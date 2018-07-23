/**
 * @license ng-notify v0.6.2
 * http://matowens.github.io/ng-notify
 * (c) 2014 MIT License, MatOwens.com
 */
(function() {
    'use strict';

    /**
     * @description
     *
     * This module provides any AngularJS application with a simple, lightweight
     * system for displaying notifications of varying degree to it's Users.
     *
     */
    var module = angular.module('ngNotify', []);

    /**
     * Check to see if the ngSanitize script has been included by the User.
     * If so, pull it in and allow for it to be used when User has specified.
     */
    var hasSanitize = false;

    try {

        /* istanbul ignore else  */
        if (angular.module('ngSanitize')) {

            // Note on the requires array from module() source code:
            // Holds the list of modules which the injector will load before the current module is loaded.

            // A sort of lazy load for our dependency on ngSanitize, only if the module exists.
            angular.module('ngNotify').requires.push('ngSanitize');

            hasSanitize = true;
        }

    } catch (err) {
        // Ignore error, we'll disable any sanitize related functionality...
    }

    // Generate ngNotify template and add it to cache...

    var html =
        '<div class="ngn" ng-class="ngNotify.notifyClass" ng-style="ngNotify.notifyStyle">' +
            '<span ng-show="ngNotify.notifyButton" class="ngn-dismiss" ng-click="dismiss()">&times;</span>' +
            '<span ng-if="ngNotify.notifyHtml" ng-bind-html="ngNotify.notifyMessage"></span>' + // Display HTML notifications.
            '<span ng-if="!ngNotify.notifyHtml" ng-bind="ngNotify.notifyMessage"></span>' + // Display escaped notifications.
        '</div>';

    module.run(['$templateCache',

        function($templateCache) {

            $templateCache.put('templates/ng-notify/ng-notify.html', html);
        }
    ]);

    module.provider('ngNotify', function() {

        this.$get = ['$document', '$compile', '$log', '$rootScope', '$timeout', '$interval', '$templateCache',

            function($document, $compile, $log, $rootScope, $timeout, $interval, $templateCache) {

                // Constants...

                var EMPTY = '';
                var SPACER = ' ';
                var DEFAULT_DURATION = 3000;
                var STICKY_CLASS = 'ngn-sticky';

                var FADE_IN_MODE = 1;
                var FADE_OUT_MODE = -1;
                var FADE_IN_DURATION = 200;
                var FADE_OUT_DURATION = 500;
                var FADE_INTERVAL = 25;

                var OPACITY_MIN = 0;
                var OPACITY_MAX = 1;

                // Defaults...

                var defaultOptions = {
                    theme: 'pure',
                    position: 'bottom',
                    duration: DEFAULT_DURATION,
                    type: 'info',
                    sticky: false,
                    button: true,
                    html: false
                };

                var defaultScope = {
                    notifyClass: '',
                    notifyMessage: '',
                    notifyStyle: {
                        display: 'none',
                        opacity: OPACITY_MIN
                    }
                };

                // Options...

                var themes = {
                    pure: EMPTY,
                    prime: 'ngn-prime',
                    pastel: 'ngn-pastel',
                    pitchy: 'ngn-pitchy'
                };

                var types = {
                    info: 'ngn-info',
                    error: 'ngn-error',
                    success: 'ngn-success',
                    warn: 'ngn-warn',
                    grimace: 'ngn-grimace'
                };

                var positions = {
                    bottom: 'ngn-bottom',
                    top: 'ngn-top'
                };

                // Fade params...

                var notifyTimeout;
                var notifyInterval;

                // Template and scope...

                var notifyScope = $rootScope.$new();
                var tpl = $compile($templateCache.get('templates/ng-notify/ng-notify.html'))(notifyScope);

                // Init our scope params...

                notifyScope.ngNotify = angular.extend({}, defaultScope);

                // Add the template to the page...

                $timeout(function() {
                    $document.find('body').append(tpl);
                });

                // Private methods...

                /**
                 * Gets what type of notification do display, eg, error, warning, etc.
                 *
                 * @param {Object} UserOpts - object containing User defined options.
                 *
                 * @return {String} - the type that will be assigned to this notification.
                 */
                var getType = function(UserOpts) {
                    var type = UserOpts.type || defaultOptions.type;
                    return (types[type] || types.info) + SPACER;
                };

                /**
                 * Gets the theme for a notification, eg, pure, pastel, etc.
                 *
                 * @param {Object} UserOpts - object containing User defined options.
                 *
                 * @return {String} - the theme that will be assigned to this notification.
                 */
                var getTheme = function(UserOpts) {
                    var theme = UserOpts.theme || defaultOptions.theme;
                    return (themes[theme] || themes.pure) + SPACER;
                };

                /**
                 * Gets the position of the notification, eg, top or bottom.
                 *
                 * @param {Object} UserOpts - object containing User defined options.
                 *
                 * @return {String} - the position that will be assigned to this notification.
                 */
                var getPosition = function(UserOpts) {
                    var position = UserOpts.position || defaultOptions.position;
                    return (positions[position] || positions.bottom) + SPACER;
                };

                /**
                 * Gets how long (in ms) to display the notification for.
                 *
                 * @param {Object} UserOpts - object containing User defined options.
                 *
                 * @return {Number} - the number of ms a fade on this notification will last.
                 */
                var getDuration = function(UserOpts) {
                    var duration = UserOpts.duration || defaultOptions.duration;
                    return angular.isNumber(duration) ? duration : DEFAULT_DURATION;
                };

                /**
                 * Gets our notification's sticky state, forcing manual dismissal when true.
                 *
                 * @param {Object} UserOpts - object containing User defined options.
                 *
                 * @return {Boolean} - whether we'll be showing a sticky notification or not.
                 */
                var getSticky = function(UserOpts) {
                    var sticky = UserOpts.sticky !== undefined ? UserOpts.sticky : defaultOptions.sticky;
                    return sticky ? true : false;
                };

                /**
                 * Gets whether or not we'd like to show the close button on our sticky notification.
                 * Notification is required to be sticky.
                 *
                 * @param {Object} UserOpts - object containing User defined options.
                 * @param {Boolean} isSticky - bool whether we'll be showing a sticky notification or not.
                 *
                 * @returns {Boolean} - whether or now we should display the close button.
                 */
                var showButton = function(UserOpts, isSticky) {
                    var showButton = UserOpts.button !== undefined ? UserOpts.button : defaultOptions.button;
                    return showButton && isSticky;
                };

                /**
                 * Gets whether or not to allow HTML binding via ngSanitize.
                 * Check to make sure ngSanitize is included in the project and warn the User if it's not.
                 *
                 * @param {Object} UserOpts - object containing User defined options.
                 *
                 * @return {Boolean} - whether we'll be using ng-bind-html or not.
                 */
                var getHtml = function(UserOpts) {

                    /* istanbul ignore if  */
                    if ((UserOpts.html || defaultOptions.html) && !hasSanitize) {

                        $log.debug(
                            "ngNotify warning: \ngSanitize couldn't be located.  In order to use the " +
                            "'html' option, be sure the ngSanitize source is included in your project."
                        );

                        return false;
                    }

                    var html = UserOpts.html !== undefined ? UserOpts.html : defaultOptions.html;
                    return html ? true : false;
                };

                /**
                 * Grabs all of the classes that our notification will need in order to display properly.
                 *
                 * @param {Object}  UserOpts - object containing User defined options.
                 * @param {Boolean} isSticky - optional bool indicating if the message is sticky or not.
                 *
                 * @returns {string}
                 */
                var getClasses = function(UserOpts, isSticky) {

                    var classes = getType(UserOpts) +
                                  getTheme(UserOpts) +
                                  getPosition(UserOpts);

                    classes += isSticky ? STICKY_CLASS : EMPTY;

                    return classes;
                };

                /**
                 * Resets our notification classes and message.
                 */
                var notifyReset = function() {
                    notifyScope.ngNotify = angular.extend({}, defaultScope);
                };

                /**
                 * Handles the fading functionality and the duration for each fade.
                 *
                 * @param {Number}   mode     - used to trigger fade in or out, adds or subtracts opacity until visible or hidden.
                 * @param {Number}   opacity  - initial opacity for our element.
                 * @param {Number}   duration - how long the fade should take to complete, in ms.
                 * @param {Function} callback - function to invoke once our fade is complete.
                 */
                var doFade = function(mode, opacity, duration, callback) {

                    var gap = FADE_INTERVAL / duration;

                    notifyScope.ngNotify.notifyStyle = {
                        display: 'block',
                        opacity: opacity
                    };

                    var func = function() {

                        opacity += mode * gap;

                        notifyScope.ngNotify.notifyStyle.opacity = opacity;

                        if (opacity <= OPACITY_MIN || OPACITY_MAX <= opacity) {

                            $interval.cancel(notifyInterval);
                            notifyInterval = false;

                            callback();
                        }
                    };

                    if (!notifyInterval) {
                        notifyInterval = $interval(func, FADE_INTERVAL);
                    }
                };

                /**
                 * Triggers a fade out, opacity from 1 to 0.
                 *
                 * @param {Number}   duration - how long the fade should take to complete, in ms.
                 * @param {Function} callback - function to invoke once fade has completed.
                 */
                var fadeOut = function(duration, callback) {
                    doFade(FADE_OUT_MODE, OPACITY_MAX, duration, callback);
                };

                /**
                 * Triggers a fade in, opacity from 0 to 1.
                 *
                 * @param  {Number}   duration - how long the fade should take to complete, in ms.
                 * @param  {Function} callback - function to invoke once fade has completed.
                 */
                var fadeIn = function(duration, callback) {
                    doFade(FADE_IN_MODE, OPACITY_MIN, duration, callback);
                };

                /**
                 * Dismisses our notification when called, attached to scope for ngCLick event to trigger.
                 */
                notifyScope.dismiss = function() {
                    fadeOut(FADE_OUT_DURATION, function() {
                        notifyReset();
                    });
                };

                /**
                 * Our primary object containing all public API methods and allows for all our functionality to be invoked.
                 *
                 * @type {Object}
                 */
                return {

                    /**
                     * Merges our User specified options with our default set of options.
                     *
                     * @param {Object} params - object of User provided options to configure notifications.
                     */
                    config: function(params) {
                        params = params || {};
                        angular.extend(defaultOptions, params);
                    },

                    /**
                     * Sets, configures and displays each notification.
                     *
                     * @param {String}                   message - the message our notification will display to the User.
                     * @param {String|Object|undefined}  UserOpt - optional parameter that contains the type or an object of options used to configure this notification.
                     * @param {String|undefined}         UserOpt.type
                     * @param {String|undefined}         UserOpt.theme
                     * @param {String|undefined}         UserOpt.position
                     * @param {Number|undefined}         UserOpt.duration
                     * @param {Boolean|undefined}        UserOpt.sticky
                     * @param {Boolean|undefined}        UserOpt.button
                     * @param {Boolean|undefined}        UserOpt.html
                     */
                    set: function(message, UserOpt) {

                        if (!message) {
                            return;
                        }

                        $interval.cancel(notifyInterval);
                        notifyInterval = false;

                        $timeout.cancel(notifyTimeout);

                        var UserOpts = {};

                        // User either provides an object of options
                        // or a string specifying the type.
                        if (typeof UserOpt === 'object') {
                            UserOpts = UserOpt;
                        } else {
                            UserOpts.type = UserOpt;
                        }

                        var isSticky = getSticky(UserOpts);
                        var duration = getDuration(UserOpts);

                        angular.extend(notifyScope.ngNotify, {
                            notifyHtml: getHtml(UserOpts),
                            notifyClass: getClasses(UserOpts, isSticky),
                            notifyButton: showButton(UserOpts, isSticky),
                            notifyMessage: message
                        });

                        fadeIn(FADE_IN_DURATION, function() {

                            if (isSticky) {
                                return;
                            }

                            notifyTimeout = $timeout(function() {
                                notifyScope.dismiss();
                            }, duration);
                        });
                    },

                    /**
                     * Allows a developer to manually dismiss a notification that may be
                     * set to sticky, when the message is no longer warranted.
                     */
                    dismiss: function() {
                        notifyScope.dismiss();
                    },

                    // User customizations...

                    /**
                     * Adds a new, User specified theme to our notification system
                     * that they can then use throughout their application.
                     *
                     * @param {String} themeName  - the name for this new theme that will be used when applying it via configuration.
                     * @param {String} themeClass - the class that this theme will use when applying it's styles.
                     */
                    addTheme: function(themeName, themeClass) {

                        if (!themeName || !themeClass) {
                            return;
                        }

                        themes[themeName] = themeClass;
                    },

                    /**
                     * Adds a new, User specified notification type that they
                     * can then use throughout their application.
                     *
                     * @param {String} typeName  - the name for this new type that will be used when applying it via configuration.
                     * @param {String} typeClass - the class that this type will use when applying it's styles.
                     */
                    addType: function(typeName, typeClass) {

                        if (!typeName || !typeClass) {
                            return;
                        }

                        types[typeName] = typeClass;
                    }
                };
            }
        ];
    });
})();
