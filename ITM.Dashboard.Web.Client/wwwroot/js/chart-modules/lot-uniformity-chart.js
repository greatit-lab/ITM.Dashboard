// ITM.Dashboard.Web.Client/wwwroot/js/chart-modules/lot-uniformity-chart.js
window.AmChartMakers = window.AmChartMakers || {};

window.AmChartMakers.LotUniformityChart = {
    create: function (root, data, config) {
        const chart = root.container.children.push(am5xy.XYChart.new(root, {
            panX: true,
            panY: true,
            wheelX: "panX",
            wheelY: "zoomY",
            pinchZoomX: true,
            pinchZoomY: true,
            layout: root.horizontalLayout
        }));

        const isDarkMode = document.body.querySelector('.dark-theme-main-content') !== null;
        const textColor = isDarkMode ? am5.color(0xffffff) : am5.color(0x000000);

        const cursor = chart.set("cursor", am5xy.XYCursor.new(root, {
            behavior: "zoomXY",
            lineY: { visible: false }
        }));

        const xAxis = chart.xAxes.push(am5xy.ValueAxis.new(root, {
            renderer: am5xy.AxisRendererX.new(root, {}),
            tooltip: am5.Tooltip.new(root, {})
        }));

        // 1. xAxis를 기본 렌더러로 먼저 생성합니다.
        const xAxis = chart.xAxes.push(am5xy.ValueAxis.new(root, {
            renderer: am5xy.AxisRendererX.new(root, {}),
            tooltip: am5.Tooltip.new(root, {})
        }));

        // ★★★ [핵심 수정] 축이 계산하는 값의 최대 정밀도를 0 (정수)으로 강제합니다. ★★★
        // 이 설정이 소수점 눈금이 생성되는 것을 원천적으로 차단합니다.
        xAxis.set("maxPrecision", 0);

        // 2. 생성된 xAxis의 renderer에 필요한 설정을 단계적으로 적용합니다.
        xAxis.get("renderer").setAll({
            minGridDistance: 40,      // 눈금 최소 간격 유지
            minorGridEnabled: false   // 보조 눈금 비활성화
        });
        
        // 3. 라벨의 표시 형식도 정수로 지정합니다.
        xAxis.get("renderer").labels.template.setAll({
            fill: textColor,
            numberFormat: "#"
        });

        xAxis.children.push(am5.Label.new(root, { text: config.xTitle, x: am5.p50, centerX: am5.p50, fill: textColor }));

        const yAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
            renderer: am5xy.AxisRendererY.new(root, {}),
            tooltip: am5.Tooltip.new(root, {})
        }));
        yAxis.get("renderer").labels.template.setAll({ fill: textColor });
        yAxis.children.moveValue(am5.Label.new(root, { text: config.yTitle, rotation: -90, y: am5.p50, centerX: am5.p50, fill: textColor }), 0);

        const legend = chart.children.push(am5.Legend.new(root, {
            centerY: am5.p50,
            y: am5.p50,
            layout: root.verticalLayout
        }));
        legend.labels.template.setAll({ fill: textColor });

        const waferIds = [...new Set(data.map(item => item.waferId))];
        const allSeries = [];

        waferIds.forEach(waferId => {
            const series = chart.series.push(am5xy.LineSeries.new(root, {
                name: "Slot " + waferId,
                xAxis: xAxis,
                yAxis: yAxis,
                valueYField: "value",
                valueXField: "point",
                tooltip: am5.Tooltip.new(root, {
                    pointerOrientation: "horizontal",
                    labelText: "[bold]{name}:[/]\nPoint: {valueX}\nValue: {valueY.formatNumber('#.00')}"
                })
            }));

            series.strokes.template.setAll({ strokeWidth: 2 });
            const waferData = data.filter(d => d.waferId === waferId);
            series.data.setAll(waferData);
            legend.data.push(series);
            allSeries.push(series);
        });

        cursor.set("snapToSeries", allSeries);

        legend.itemContainers.template.events.on("pointerover", function (e) {
            let series = e.target.dataItem.dataContext;
            chart.series.each(function (chartSeries) {
                if (chartSeries != series) {
                    chartSeries.strokes.template.setAll({ strokeOpacity: 0.3, stroke: am5.color(0x888888) });
                } else {
                    chartSeries.strokes.template.set("strokeWidth", 3);
                }
            });
        });

        legend.itemContainers.template.events.on("pointerout", function (e) {
            chart.series.each(function (chartSeries) {
                chartSeries.strokes.template.setAll({ strokeOpacity: 1, strokeWidth: 2, stroke: chartSeries.get("stroke") });
            });
        });

        const scrollbarX = chart.set("scrollbarX", am5xy.XYChartScrollbar.new(root, {
            orientation: "horizontal",
            height: 50
        }));

        const sbxAxis = scrollbarX.chart.xAxes.push(am5xy.ValueAxis.new(root, { renderer: am5xy.AxisRendererX.new(root, {}) }));
        const sbyAxis = scrollbarX.chart.yAxes.push(am5xy.ValueAxis.new(root, { renderer: am5xy.AxisRendererY.new(root, {}) }));

        sbxAxis.get("renderer").labels.template.setAll({ numberFormat: "#" });

        waferIds.forEach(waferId => {
            const sbSeries = scrollbarX.chart.series.push(am5xy.LineSeries.new(root, {
                xAxis: sbxAxis, yAxis: sbyAxis, valueYField: "value", valueXField: "point"
            }));
            const waferData = data.filter(d => d.waferId === waferId);
            sbSeries.data.setAll(waferData);
        });

        const scrollbarY = chart.set("scrollbarY", am5xy.XYChartScrollbar.new(root, {
            orientation: "vertical",
            width: 50
        }));

        const sbxAxis_y = scrollbarY.chart.xAxes.push(am5xy.ValueAxis.new(root, { renderer: am5xy.AxisRendererX.new(root, {}) }));
        const sbyAxis_y = scrollbarY.chart.yAxes.push(am5xy.ValueAxis.new(root, { renderer: am5xy.AxisRendererY.new(root, {}) }));

        sbxAxis_y.get("renderer").labels.template.setAll({ numberFormat: "#" });

        waferIds.forEach(waferId => {
            const sbSeries = scrollbarY.chart.series.push(am5xy.LineSeries.new(root, {
                xAxis: sbxAxis_y, yAxis: sbyAxis_y, valueYField: "value", valueXField: "point"
            }));
            const waferData = data.filter(d => d.waferId === waferId);
            sbSeries.data.setAll(waferData);
        });

        chart.appear(1000, 100);
    }
};
