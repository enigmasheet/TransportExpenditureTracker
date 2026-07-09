$(document).ready(function () {
    function loadChart(url, canvasId, label, color, type) {
        var spinnerId = canvasId + 'Spinner';
        var $spinner = $('#' + spinnerId);
        var $canvas = $('#' + canvasId);

        $.getJSON(url)
            .done(function (data) {
                $spinner.addClass('d-none');
                $canvas.removeClass('d-none');
                var ctx = document.getElementById(canvasId).getContext('2d');
                new Chart(ctx, {
                    type: type || 'bar',
                    data: {
                        labels: data.map(function (d) { return d.label || d.month || d.category || d.supplier; }),
                        datasets: [{
                            label: label,
                            data: data.map(function (d) { return d.amount || d.vatAmount || d.value || 0; }),
                            backgroundColor: color || 'rgba(13, 110, 253, 0.6)',
                            borderColor: 'rgba(13, 110, 253, 1)',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: { legend: { display: false } },
                        scales: { y: { beginAtZero: true, ticks: { callback: function (v) { return 'Rs ' + v.toLocaleString('en-IN'); } } } }
                    }
                });
            })
            .fail(function () {
                $spinner.html('<div class="text-danger">Failed to load chart. <a href="#" onclick="location.reload();return false;">Retry</a></div>');
            });
    }
    loadChart('/dashboard/GetMonthlyChartData', 'monthlyChart', 'Monthly Expenditure', 'rgba(13, 110, 253, 0.6)');
    loadChart('/dashboard/GetCategoryChartData', 'categoryChart', 'By Category', ['#ff6384', '#36a2eb', '#ffce56', '#4bc0c0', '#9966ff', '#ff9f40'], 'doughnut');
    loadChart('/dashboard/GetSupplierChartData', 'supplierChart', 'Top Suppliers', 'rgba(40, 167, 69, 0.6)');
    loadChart('/dashboard/GetVatChartData', 'vatChart', 'VAT Paid', 'rgba(255, 193, 7, 0.6)');
    loadChart('/dashboard/GetFyComparisonData', 'fyChart', 'FY Comparison', 'rgba(111, 66, 193, 0.6)');
});
