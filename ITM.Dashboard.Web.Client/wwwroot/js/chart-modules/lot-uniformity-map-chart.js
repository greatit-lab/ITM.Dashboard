// ITM.Dashboard.Web.Client/wwwroot/js/chart-modules/lot-uniformity-map-chart.js
window.AmChartMakers = window.AmChartMakers || {};

window.AmChartMakers.LotUniformityMapChart = {
    create: function (root, data, config) {
        const chart = root.container.children.push(am5xy.XYChart.new(root, {
            panX: true,
            panY: true,
            wheelX: "panX",
            wheelY: "zoomXY",
            pinchZoomX: true,
            pinchZoomY: true,
            maxTooltipDistance: 0
        }));

        const isDarkMode = document.body.querySelector('.dark-theme-main-content') !== null;
        const textColor = isDarkMode ? am5.color(0xffffff) : am5.color(0x000000);
        const gridColor = isDarkMode ? am5.color(0x555555) : am5.color(0xDDDDDD);

        // X축: -7 ~ 7 범위 고정, 격자는 Axis Ranges로 직접 그림
        const xAxis = chart.xAxes.push(am5xy.ValueAxis.new(root, {
            min: -7,
            max: 7,
            strictMinMax: true,
            renderer: am5xy.AxisRendererX.new(root, {
                strokeOpacity: 0,
                // 기본 격자는 숨깁니다.
                grid: { template: { strokeOpacity: 0 } }
            }),
            tooltip: am5.Tooltip.new(root, {})
        }));
        xAxis.get("renderer").labels.template.set("forceHidden", true);

        // Y축: -7 ~ 7 범위 고정, 격자는 Axis Ranges로 직접 그림
        const yAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
            min: -7,
            max: 7,
            strictMinMax: true,
            renderer: am5xy.AxisRendererY.new(root, {
                strokeOpacity: 0,
                // 기본 격자는 숨깁니다.
                grid: { template: { strokeOpacity: 0 } }
            }),
            tooltip: am5.Tooltip.new(root, {})
        }));
        yAxis.get("renderer").labels.template.set("forceHidden", true);


        // ▼▼▼ [핵심 수정] Axis Ranges를 사용하여 2단위 격자를 직접 그립니다. ▼▼▼
        function createGridLine(axis, value) {
            const rangeDataItem = axis.makeDataItem({
                value: value
            });

            axis.createAxisRange(rangeDataItem);

            rangeDataItem.get("grid").setAll({
                stroke: gridColor,
                strokeOpacity: 0.2,
                visible: true
            });
        }

        // -6, -4, -2, 0, 2, 4, 6 위치에 세로선(X축 격자)을 그립니다.
        for (let i = -6; i <= 6; i += 1) {
            createGridLine(xAxis, i);
        }
        // -6, -4, -2, 0, 2, 4, 6 위치에 가로선(Y축 격자)을 그립니다.
        for (let i = -6; i <= 6; i += 1) {
            createGridLine(yAxis, i);
        }
        // ▲▲▲ [핵심 수정] 여기까지 ▲▲▲


        // 원형 보조 테두리
        const circleData = [];
        for (let i = 0; i <= 360; i += 5) {
            const angle = i * Math.PI / 180;
            circleData.push({ x: 6 * Math.cos(angle), y: 6 * Math.sin(angle) });
        }
        const circleSeries = chart.series.push(am5xy.LineSeries.new(root, {
            xAxis: xAxis,
            yAxis: yAxis,
            valueXField: "x",
            valueYField: "y",
            stroke: am5.color(isDarkMode ? 0x555555 : 0xAAAAAA),
            strokeWidth: 2,
            tooltip: am5.Tooltip.new(root, { labelText: "" })
        }));
        circleSeries.data.setAll(circleData);
        circleSeries.set("interactive", false);

        // 데이터 시리즈
        const series = chart.series.push(am5xy.LineSeries.new(root, {
            xAxis: xAxis,
            yAxis: yAxis,
            valueXField: "mapX",
            valueYField: "mapY",
            valueField: "point",
            tooltip: am5.Tooltip.new(root, {
                labelText: "[bold]Point #{point}[/]\nMetric: {value.formatNumber('#.00')}\n(X:{mapX}, Y:{mapY})"
            })
        }));

        series.strokes.template.set("forceHidden", true);

        // 데이터 포인트와 라벨을 그리는 bullet 설정
        series.bullets.push(function () {
            const container = am5.Container.new(root, {});

            container.children.push(am5.Circle.new(root, {
                radius: 5,
                fill: am5.color(config.pointColor || "#81C784"),
                stroke: am5.color(isDarkMode ? 0x000000 : 0xffffff),
                strokeWidth: 1
            }));

            const label = container.children.push(am5.Label.new(root, {
                text: "{value}",
                populateText: true,
                fontSize: "0.8em",
                fill: textColor,
                centerX: am5.p50,
                dy: 15
            }));

            return am5.Bullet.new(root, {
                sprite: container,
                alwaysShow: true
            });
        });

        series.data.setAll(data);
        chart.appear(1000, 100);
    }
};
