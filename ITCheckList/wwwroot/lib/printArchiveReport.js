function printArchiveReport() {
    executeWithOverlay(() => {
        const table = document.getElementById("archiveTable");
        const selectedDate = document.getElementById("selectedDate").value.trim();

        if (!selectedDate) {
            Swal.fire({
                icon: 'warning',
                title: 'خطا',
                text: 'لطفاً جهت نمایش اطلاعات بایگانی، تاریخ خود را انتخاب نمایید.'
            });
            return;
        }

        if (!table || table.rows.length <= 1) {
            Swal.fire({
                icon: 'warning',
                title: 'بدون داده',
                text: 'داده‌ای برای چاپ یافت نشد. ابتدا گزارش بایگانی را مشاهده نمایید.'
            });
            return;
        }

        const clonedTable = table.cloneNode(true);

        Array.from(clonedTable.rows).forEach(row => {
            if (row.cells.length > 0 && row.cells[row.cells.length - 1].textContent.includes("ویرایش")) {
                row.deleteCell(row.cells.length - 1);
            }
        });

        Array.from(clonedTable.rows).forEach(row => {
            const statusCell = row.cells[5];
            if (statusCell && statusCell.textContent.includes("انجام شد")) {
                row.style.backgroundColor = "#0056B314";
            }
        });

        const now = new Date();
        const faDate = now.toLocaleDateString('fa-IR');
        const time = now.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' });

        const header = `<div class="print-header">گزارش بایگانی چک‌لیست</div>`;
        const subheader = `<div class="print-subtitle">تاریخ انتخاب‌شده: ${selectedDate}</div>`;
        const footer = `<div class="print-footer">تاریخ و ساعت چاپ: ${faDate} - ${time}</div>`;

        const printableHTML = `
            ${header}
            ${subheader}
            ${clonedTable.outerHTML}
            ${footer}
        `;

        const printStyle = `
            @font-face {
                font-family: 'IRANYekanXFaNum';
                src: url('/fonts/IRANYekanXFaNum-Regular.woff2') format('truetype');
            }
            body {
                direction: rtl;
                font-family: 'IRANYekanXFaNum';
            }
            .print-header {
                text-align: center;
                font-size: 18px;
                margin-bottom: 5px;
                font-family: 'IRANYekanXFaNum';
                font-weight: bold;
            }
            .print-subtitle {
                text-align: center;
                font-size: 14px;
                margin-bottom: 15px;
                color: #555;
            }
            .print-footer {
                margin-top: 20px;
                text-align: center;
                font-size: 12px;
                color: #333;
            }
            table {
                width: 100%;
                border-collapse: collapse;
                font-size: 13px;
                margin-top: 10px;
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
            @page {
                margin: 10mm;
            }
        `;

        printJS({
            printable: printableHTML,
            type: "raw-html",
            style: printStyle
        });
    });
}
