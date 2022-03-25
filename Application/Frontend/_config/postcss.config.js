const autoprefixer = require('autoprefixer');

module.exports = {
    syntax: 'postcss-scss',
    plugins: [
        autoprefixer({ 
            flexbox: true,
            overrideBrowserslist: [
                '>= 0.5%',
                'last 2 major versions',
                'not dead',
                'Chrome >= 60',
                'Firefox >= 60',
                'Firefox ESR',
                'iOS >= 12',
                'Safari >= 12',
                'not Explorer <= 11',
            ]
        }),
    ]
}
