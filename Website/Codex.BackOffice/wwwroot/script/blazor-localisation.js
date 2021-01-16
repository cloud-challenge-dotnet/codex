window.browserCulture = {
    get: function () {
        return navigator.languages && navigator.languages.length ? navigator.languages[0] : navigator['userLanguage']
            || navigator.language
            || navigator['browserLanguage']
            || 'en';
    }
};

window.selectedCulture = {
    get: () => localStorage['SelectedCulture'],
    set: (value) => localStorage['SelectedCulture'] = value
};