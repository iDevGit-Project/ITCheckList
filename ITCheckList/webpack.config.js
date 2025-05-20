const path = require('path');

module.exports = {
    entry: './wwwroot/assets/js/main.js', // مسیر ورودی JS
    output: {
        path: path.resolve(__dirname, 'wwwroot/dist'),
        filename: 'bundle.js',
        publicPath: '/dist/' // آدرس عمومی برای سرو کردن فایل‌ها
    },
    mode: 'development',
    devServer: {
        static: {
            directory: path.join(__dirname, 'wwwroot'),
        },
        devMiddleware: {
            publicPath: '/dist/',
        },
        compress: true,
        port: 3000,
        hot: true
    }
};
