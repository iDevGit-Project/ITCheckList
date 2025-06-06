const path = require("path");

module.exports = {
  mode: "production", // برای نسخه نهایی (production)، یا 'development' برای حالت توسعه

  entry: "./wwwroot/js/src/index.js", // نقطه شروع اصلی فایل‌های JS
  output: {
    filename: "bundle.min.js", // نام فایل خروجی
    path: path.resolve(__dirname, "./wwwroot/dist"), // مسیر خروجی
    clean: true, // حذف فایل‌های خروجی قبلی هنگام Build جدید
  },

  module: {
    rules: [
      {
        test: /\.js$/, // تمام فایل‌های .js
        exclude: /node_modules/, // به جز node_modules
        use: {
          loader: "babel-loader", // تبدیل کد ES6+ به ES5
          options: {
            presets: ["@babel/preset-env"],
          },
        },
      },
    ],
  },

  watch: true, // فعال کردن watch برای تغییرات خودکار
};
