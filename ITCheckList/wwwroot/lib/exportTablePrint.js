function printFullTable() {
    fetch('/Checklist/IsCheckItemTableEmpty')
        .then(response => {
            if (!response.ok) {
                throw new Error('خطا در دریافت اطلاعات جدول.');
            }
            return response.json();
        })
        .then(data => {
            if (data.isEmpty) {
                Swal.fire({
                    icon: 'warning',
                    title: 'هشدار!',
                    html: '<div style="direction: rtl;">هیچ داده‌ای برای دریافت خروجی وجود ندارد.</div>',
                    confirmButtonText: 'متوجه شدم',
                    customClass: {
                        popup: 'swal2-rtl'
                    }
                });
            } else {
                // اجرای پرینت فقط زمانی که داده وجود دارد
                const table = document.getElementById("reportTable");
                if (!table) {
                    alert("جدول پیدا نشد.");
                    return;
                }

                const clonedTable = table.cloneNode(true);

                // حذف ستون عملیات
                Array.from(clonedTable.rows).forEach(row => {
                    if (row.cells.length > 0) {
                        row.deleteCell(row.cells.length - 1);
                    }
                });

                const base64Font = `
                    @font-face {
                        font-family: 'IRANYekan';
                        src: url('/fonts/IRANYekanXFaNum-Regular.ttf') format('truetype');
                        font-weight: normal;
                        font-style: normal;
                    }
                `;

                const printStyle = `
                    ${base64Font}
                    body {
                        direction: rtl;
                        font-family: 'IRANYekan', sans-serif;
                    }
                    .print-header {
                        text-align: center;
                        font-size: 18px;
                        margin-bottom: 10px;
                        font-weight: bold;
                    }
                    table {
                        width: 100%;
                        border-collapse: collapse;
                        font-size: 13px;
                        direction: rtl;
                    }
                    th, td {
                        border: 1px solid black;
                        padding: 8px;
                    }
                    th {
                        background-color: #f8f9fa;
                        font-weight: bold;
                    }
                    @page {
                        margin: 8mm;
                    }
                `;

                const header = '<div class="print-header">چک لیست بررسی تجهیزات دپارتمان واحد فناوری اطلاعات</div>';

                printJS({
                    printable: header + clonedTable.outerHTML,
                    type: "raw-html",
                    header: "",
                    style: printStyle,
                });
            }
        })
        .catch(error => {
            Swal.fire({
                icon: 'error',
                title: 'خطا!',
                html: `<div style="direction: rtl;">${error.message}</div>`,
                confirmButtonText: 'باشه'
            });
        });
}



//function printFullTable() {
//    const table = document.getElementById("reportTable");
//    if (!table) {
//        alert("جدول پیدا نشد.");
//        return;
//    }

//    const clonedTable = table.cloneNode(true);

//    // حذف ستون عملیات
//    Array.from(clonedTable.rows).forEach(row => {
//        if (row.cells.length > 0) {
//            row.deleteCell(row.cells.length - 1);
//        }
//    });

//    // IRANYekan Base64 - نسخه Regular (برای ساده‌سازی فقط یک نسخه)
//    const base64Font = `
//        @font-face {
//            font-family: 'IRANYekan';
//            src: url('/fonts/IRANYekanXFaNum-Regular.ttf') format('truetype');
//            font-weight: normal;
//            font-style: normal;
//        }
//    `;

//    const printStyle = `
//        ${base64Font}
//        body {
//            direction: rtl;
//            font-family: 'IRANYekan', sans-serif;
//        }
//        .print-header {
//            text-align: center;
//            font-size: 18px;
//            margin-bottom: 10px;
//            font-weight: bold;
//        }
//        table {
//            width: 100%;
//            border-collapse: collapse;
//            font-size: 13px;
//            direction: rtl;
//        }
//        th, td {
//            border: 1px solid black;
//            padding: 8px;
//        }
//        th {
//            background-color: #f8f9fa;
//            font-weight: bold;
//        }
//        @page {
//            margin: 8mm;
//        }
//    `;

//    const header = '<div class="print-header">چک لیست بررسی تجهیزات دپارتمان واحد فناوری اطلاعات</div>';

//    printJS({
//        printable: header + clonedTable.outerHTML,
//        type: "raw-html",
//        header: "", // می‌توانیم عنوان در اینجا هم قرار بدهیم ولی از div استفاده کرده‌ایم
//        header: "خروجی جدول",
//        style: printStyle,
//    });
//}

