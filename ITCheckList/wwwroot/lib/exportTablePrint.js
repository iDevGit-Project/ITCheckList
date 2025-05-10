// اطمینان از بارگذاری DOM قبل از اجرای کد
document.addEventListener("DOMContentLoaded", () => {
    const exportMenu = document.getElementById("kt_datatable_export_menu");
    const exportConfirmationModal = new bootstrap.Modal(document.getElementById("exportConfirmationModal"));
    const rowInput = document.getElementById("rowIndices");
    const columnInput = document.getElementById("columnIndices");
    let selectedExportType = null;

    // محدود کردن ورودی به عدد و کاما
    function restrictToNumbersAndEnglish(inputElement) {
        inputElement.addEventListener("input", () => {
            inputElement.value = inputElement.value.replace(/[^0-9,]/g, ""); // فقط اعداد و کاما مجاز است
        });
    }

    restrictToNumbersAndEnglish(rowInput);
    restrictToNumbersAndEnglish(columnInput);

    exportMenu.addEventListener("click", (e) => {
        e.preventDefault();
        const target = e.target.closest("[data-kt-export]");
        if (!target) return;
        selectedExportType = target.getAttribute("data-kt-export");

        // نمایش مودال تایید
        exportConfirmationModal.show();
    });

    // دکمه تایید برای عملیات
    document.getElementById("confirmExport").addEventListener("click", async () => {
        const rowIndices = rowInput.value
            ? rowInput.value.split(",").map((i) => parseInt(i.trim())).filter((i) => !isNaN(i))
            : [];
        const columnIndices = columnInput.value
            ? columnInput.value.split(",").map((i) => parseInt(i.trim())).filter((i) => !isNaN(i))
            : [];

        exportConfirmationModal.hide();

        Swal.fire({
            icon: "success",
            title: "عملیات موفق",
            text: "عملیات خروجی با موفقیت انجام شد.",
            buttonsStyling: false,
            confirmButtonText: "ثبت و بازگشت",
            customClass: { confirmButton: "btn btn-sm btn-light-success" },
        }).then(() => {
            if (selectedExportType === "pdf") {
                generatePDFWithPrintJS("datatable_Discounts", rowIndices, columnIndices);
            }
        });
    });

    document.getElementById("cancelExport").addEventListener("click", () => {
        exportConfirmationModal.hide();
        Swal.fire({
            title: "انصراف",
            text: "عملیات خروجی فایل لغو شد.",
            buttonsStyling: false,
            confirmButtonText: "قبول و بازگشت",
            customClass: { confirmButton: "btn btn-sm btn-danger" },
        });
    });
});

// بارگذاری و اعمال فونت IRANYekan با استفاده از FontFace API
const loadIRANYekanFont = () => {
    const fontRegular = new FontFace('IRANYekan', 'url(/AdminTheme/assets/fonts/IRANYekanXFaNum-Regular.ttf)');
    const fontBold = new FontFace('IRANYekan', 'url(/AdminTheme/assets/fonts/IRANYekanXFaNum-Bold.ttf)', { weight: 'bold' });

    // بارگذاری فونت‌ها
    return Promise.all([fontRegular.load(), fontBold.load()])
        .then(() => {
            document.fonts.add(fontRegular);
            document.fonts.add(fontBold);
        })
        .catch((error) => {
            console.error('خطا در بارگذاری فونت‌ها:', error);
        });
};

// تابع تولید PDF با Print.js
function generatePDFWithPrintJS(tableId, rowsToHide = [], columnsToHide = []) {
    const table = document.getElementById(tableId);
    if (!table) {
        Swal.fire({
            title: "خطا",
            text: "جدول مورد نظر پیدا نشد.",
            icon: "error",
            buttonsStyling: false,
            confirmButtonText: "بازگشت",
            customClass: { confirmButton: "btn btn-sm btn-danger" },
        });
        return;
    }

    // اعمال فیلتر روی سطرها و ستون‌ها
    const filteredTable = table.cloneNode(true);
    const rows = Array.from(filteredTable.rows);

    rows.forEach((row, rowIndex) => {
        if (rowsToHide.includes(rowIndex)) {
            row.remove();
        } else {
            Array.from(row.cells).forEach((cell, colIndex) => {
                if (columnsToHide.includes(colIndex)) {
                    cell.remove();
                }
            });
        }
    });

    // تنظیمات سبک پرینت
    const RTL_TableStyle = `
        table {
            width: 100%;
            border-collapse: collapse;
            direction: rtl; /* راست‌چین */
            font-family: 'IRANYekan', sans-serif; /* استفاده از فونت فارسی */
            border: 1px solid black; /* حاشیه برای جدول */
        }
        th, td {
            border: 1px solid black;
            padding: 8px;
            text-align: center;
        }
        th {
            background-color: #f8f9fa;
            font-weight: bold;
        }
     /* اضافه کردن عنوان بالای صفحه */
        .print-header {
            text-align: center;
            font-size: 18px;
            margin-bottom: 10px;
            font-family: 'IRANYekan', sans-serif; /* استفاده از فونت فارسی */
            font-weight: bold;
        }
     /* اضافه کردن حاشیه به صفحه پرینت */
        @page {
            margin: 8mm; /* فاصله از لبه‌های صفحه */
            border: 1px solid black; /* حاشیه برای صفحه */
        }
    `;

    // اضافه کردن فونت‌ها به استایل پرینت
    const printStyle = `
        ${RTL_TableStyle}
        @font-face {
            font-family: 'IRANYekan';
            src: url('/AdminTheme/assets/fonts/IRANYekanXFaNum-Regular.ttf') format('truetype');
            font-weight: normal;
            font-style: normal;
        }
        @font-face {
            font-family: 'IRANYekan';
            src: url('/AdminTheme/assets/fonts/IRANYekanXFaNum-Bold.ttf') format('truetype');
            font-weight: bold;
            font-style: normal;
        }
    `;

    // فراخوانی برای پرینت پس از بارگذاری کامل فونت‌ها
    loadIRANYekanFont().then(() => {
        // اضافه کردن عنوان بالای صفحه
        const header = '<div class="print-header">چاپ اطلاعات</div>';

        printJS({
            printable: header + filteredTable.outerHTML,
            type: "raw-html",
            header: "", // می‌توانیم عنوان در اینجا هم قرار بدهیم ولی از div استفاده کرده‌ایم
            header: "خروجی جدول",
            style: printStyle,
        });
    });
}

