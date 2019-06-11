
function InitGoogleChart() {
    $.ajaxSetup({
        converters: {
            "text json": function (text) {
                return JSON.parse(text, reviveDate);
            }
        }
    });
}
function reviveDate(key, val) {
    if (key === 'v') {
        if (val !== null && val.constructor === String) {
            //XXXX-XX-XXTXX:XX:XX
            //XXXX-XX-XXTXX:XX:XX.X
            //XXXX-XX-XXTXX:XX:XX.XX
            //XXXX-XX-XXTXX:XX:XX.XXX
            //XXXX-XX-XXTXX:XX:XX+XX:XX
            //XXXX-XX-XXTXX:XX:XX.XX-XX:XX
            if (val.match(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}($|\.\d{1,3}($|[+-]\d{2}:\d{2}$))/) !== null) {
                return new Date(val);
            }
        }
    }
    return val;
}

/*
 option = {
    url:
    elementId:
}
 */
function ViewChart(option) {

    InitGoogleChart();

    $.getJSON(option.url, option.data, function (data) {
        DrawDataLineChart(data);
    })
        .fail(function (xhr, status, error) {
            window.alert('error:' + error);
        });

    function DrawDataLineChart(data) {
        // グラフを描画するためのパッケージ(corechart)の呼び出し--------------①
        google.charts.load('current', { 'packages': ['corechart'] });

        // コールバック--------------②
        google.charts.setOnLoadCallback(function () {
            var title = 'Title';
            var dataTable = new google.visualization.DataTable(data);
            drawLineChart(option.elementId, title, dataTable);
        });

        function drawLineChart(elementId, title, dataTable) {
            // データの作成--------------③
            // グラフの描画--------------④
            var options = {
                width: '100%',
                height: '100%',
                0: {
                    color: 'black',
                    lineWidth: 1,
                    visibleInLegend: true
                }
            };
            var chart = new google.visualization.LineChart(document.getElementById(elementId));
            chart.draw(dataTable, options);
        }
    }
}

function ViewTable(option) {

    InitGoogleChart();

    $.getJSON(option.url, null, function (data) {
        DrawDatatable(data);
    })
        .fail(function (xhr, status, error) {
            window.alert('error:' + error);
        });

    function DrawDatatable(data) {
        // グラフを描画するためのパッケージ(corechart)の呼び出し--------------①
        google.charts.load('current', { 'packages': ['table'] });

        // コールバック--------------②
        google.charts.setOnLoadCallback(drawTable);

        function drawTable() {
            // データの作成--------------③
            var dataTable = new google.visualization.DataTable(data);
            // グラフの描画--------------④
            var options = {
                width: '100%',
                height: '100%',
                sort: 'disable',
                page: 'enable',
                pageSize: option.pageSize,
                showRowNumber: true,
                frozenColumns: 1,
                startPage: (data.rows.length - 1) / option.pageSize + 1
            };
            var chart = new google.visualization.Table(document.getElementById(option.elementId));
            chart.draw(dataTable, options);
        }
    }
}
