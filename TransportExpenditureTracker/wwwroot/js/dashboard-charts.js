const chartInstances = {};
const colorPalette = [
    'rgba(37, 99, 235, 0.7)', 'rgba(22, 163, 74, 0.7)', 'rgba(217, 119, 6, 0.7)',
    'rgba(8, 145, 178, 0.7)', 'rgba(124, 58, 237, 0.7)', 'rgba(220, 38, 38, 0.7)',
    'rgba(236, 72, 153, 0.7)', 'rgba(14, 165, 233, 0.7)', 'rgba(168, 85, 247, 0.7)',
    'rgba(249, 115, 22, 0.7)', 'rgba(6, 182, 212, 0.7)', 'rgba(34, 197, 94, 0.7)'
];
const borderPalette = colorPalette.map(function (c) { return c.replace('0.7', '1'); });

function qs(params) {
    const parts = [];
    for (const k in params) {
        if (params[k]) parts.push(encodeURIComponent(k) + '=' + encodeURIComponent(params[k]));
    }
    return parts.length ? '?' + parts.join('&') : '';
}

function loadChart(url, canvasId, label, type, opts) {
    opts = opts || {};
    const spinnerId = canvasId + 'Spinner';
    const emptyId = canvasId + 'Empty';
    const $spinner = $('#' + spinnerId);
    const $empty = $('#' + emptyId);
    const $canvas = $('#' + canvasId);

    const params = {};
    if (typeof window.fiscalYear !== 'undefined' && window.fiscalYear) params.fiscalYear = window.fiscalYear;

    $.getJSON(url + qs(params))
        .done(function (data) {
            $spinner.addClass('d-none');
            if (!data || data.length === 0) {
                $empty.removeClass('d-none');
                return;
            }
            $canvas.removeClass('d-none');
            const ctx = document.getElementById(canvasId).getContext('2d');

            if (chartInstances[canvasId]) {
                chartInstances[canvasId].destroy();
            }

            const labels = data.map(function (d) { return d.label; });
            const values = data.map(function (d) { return d.value; });

            const config = {
                type: type || 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: label,
                        data: values,
                            backgroundColor: opts.colors || colorPalette.slice(0, labels.length),
                            borderColor: opts.borders || borderPalette.slice(0, labels.length),
                        borderWidth: opts.doughnut ? 2 : 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            display: opts.showLegend || type === 'doughnut',
                            position: 'bottom',
                            labels: { boxWidth: 12, padding: 12, font: { size: 11 } }
                        },
                        tooltip: {
                            callbacks: {
                                label: function (ctx) {
                                    return ' Rs ' + Number(ctx.parsed).toLocaleString('en-IN', { minimumFractionDigits: 2 });
                                }
                            }
                        }
                    },
                    scales: type !== 'doughnut' ? {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: function (v) { return 'Rs ' + v.toLocaleString('en-IN'); }
                            }
                        }
                    } : {}
                }
            };

            chartInstances[canvasId] = new Chart(ctx, config);
        })
        .fail(function () {
            $spinner.html('<div class="text-danger py-3"><i class="bi bi-exclamation-triangle"></i> Failed to load chart. <a href="#" onclick="location.reload();return false;">Retry</a></div>');
        });
}

$(document).ready(function () {
    loadChart('/dashboard/GetMonthlyChartData', 'monthlyChart', 'Monthly Expenditure', 'bar');
    loadChart('/dashboard/GetCategoryChartData', 'categoryChart', 'By Category', 'doughnut', { showLegend: true, doughnut: true });
    loadChart('/dashboard/GetSupplierChartData', 'supplierChart', 'Top Suppliers', 'bar', { colors: ['rgba(22, 163, 74, 0.7)'], borders: ['rgba(22, 163, 74, 1)'] });
    loadChart('/dashboard/GetVatChartData', 'vatChart', 'VAT Paid', 'bar', { colors: ['rgba(217, 119, 6, 0.7)'], borders: ['rgba(217, 119, 6, 1)'] });
    loadChart('/dashboard/GetFyComparisonData', 'fyChart', 'FY Comparison', 'bar', { colors: ['rgba(124, 58, 237, 0.7)'], borders: ['rgba(124, 58, 237, 1)'] });
});
