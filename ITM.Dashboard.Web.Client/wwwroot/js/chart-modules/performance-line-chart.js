// ITM.Dashboard.Web.Client/wwwroot/js/chart-modules/performance-line-chart.js

window.AmChartMakers = window.AmChartMakers || {};

// Performance Trend 페이지 전용 차트 Maker
window.AmChartMakers.PerformanceLineChart = {
    create: function (root, data, config) {
        const chart = root.container.children.push(am5xy.XYChart.new(root, {
            panX: true, panY: true, wheelX: "panX", wheelY: "zoomX",
            layout: root.verticalLayout
        }));

        const isDarkMode = document.body.querySelector('.dark-theme-main-content') !== null;
        const textColor = isDarkMode ? am5.color(0xffffff) : am5.color(0x000000);
        const gridColor = isDarkMode ? am5.color(0xffffff) : am5.color(0x000000);
        const cursorColor = isDarkMode ? am5.color(0xffffff) : am5.color(0x000000);

        const cursor = chart.set("cursor", am5xy.XYCursor.new(root, { behavior: "zoomXY" }));
        cursor.lineX.set("stroke", cursorColor);
        cursor.lineY.set("stroke", cursorColor);

        const xAxis = chart.xAxes.push(am5xy.DateAxis.new(root, {
            baseInterval: { timeUnit: "second", count: 1 },
            renderer: am5xy.AxisRendererX.new(root, { minGridDistance: 120 }),
            inputDateFormat: "yyyy-MM-ddTHH:mm:ss"
        }));

        const consistentFormat = config.xAxisDateFormat || "yy-MM-dd HH:mm";
        xAxis.get("dateFormats")["second"] = "HH:mm:ss";
        xAxis.get("dateFormats")["minute"] = consistentFormat;
        xAxis.get("dateFormats")["hour"] = consistentFormat;
        xAxis.get("dateFormats")["day"] = consistentFormat;
        xAxis.get("dateFormats")["week"] = consistentFormat;
        xAxis.get("dateFormats")["month"] = "yyyy-MM";
        xAxis.get("periodChangeDateFormats")["second"] = "HH:mm:ss";
        xAxis.get("periodChangeDateFormats")["minute"] = consistentFormat;
        xAxis.get("periodChangeDateFormats")["hour"] = consistentFormat;
        xAxis.get("periodChangeDateFormats")["day"] = consistentFormat;
        xAxis.get("periodChangeDateFormats")["week"] = consistentFormat;
        xAxis.get("periodChangeDateFormats")["month"] = "yyyy-MM";

        xAxis.get("renderer").labels.template.setAll({ fill: textColor, rotation: -45, centerY: am5.p50, centerX: am5.p100, paddingRight: 10 });
        xAxis.get("renderer").grid.template.setAll({ stroke: gridColor, strokeOpacity: 0.15 });

        // Y축 설정
        config.yAxes.forEach(yConfig => {
            const yRenderer = am5xy.AxisRendererY.new(root, { opposite: yConfig.opposite || false });
            yRenderer.labels.template.set("fill", textColor);
            yRenderer.grid.template.setAll({ stroke: gridColor, strokeOpacity: 0.15 });

            const yAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
                renderer: yRenderer,
                axisTitle: am5.Label.new(root, {
                    text: yConfig.title,
                    fill: textColor,
                    rotation: -90,
                    fontSize: "0.8em",
                    fontWeight: "500"
                })
            }));

            if (yConfig.min !== undefined) yAxis.set("min", yConfig.min);
            if (yConfig.max !== undefined) yAxis.set("max", yConfig.max);
        });

        // 시리즈 설정
        config.series.forEach(seriesConfig => {
            const tooltip = am5.Tooltip.new(root, {
                labelText: seriesConfig.tooltipText,
                getFillFromSprite: true,
                getLabelFillFromSprite: true
            });

            const series = chart.series.push(am5xy.LineSeries.new(root, {
                name: seriesConfig.name,
                xAxis: xAxis,
                yAxis: chart.yAxes.getIndex(seriesConfig.yAxisIndex || 0),
                valueYField: seriesConfig.valueField,
                valueXField: config.xField,
                stroke: am5.color(seriesConfig.color),
                fill: am5.color(seriesConfig.color),
                tooltip: tooltip
            }));
            series.strokes.template.setAll({ strokeWidth: seriesConfig.strokeWidth || 2 });

            if (seriesConfig.bulletRadius > 0) {
                series.bullets.push(() => am5.Bullet.new(root, {
                    sprite: am5.Circle.new(root, {
                        radius: seriesConfig.bulletRadius,
                        fill: series.get("stroke")
                    })
                }));
            }
            series.data.processor = am5.DataProcessor.new(root, { dateFields: [config.xField], dateFormat: "yyyy-MM-ddTHH:mm:ss" });
            series.data.setAll(data);
        });

        const legend = chart.children.push(am5.Legend.new(root, {
            centerX: am5.p50,
            x: am5.p50
        }));
        legend.labels.template.set("fill", textColor);
        legend.data.setAll(chart.series.values);
    }
};
