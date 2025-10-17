// ITM.Dashboard.Web.Client/wwwroot/js/chart-modules/process-memory-chart.js
window.AmChartMakers = window.AmChartMakers || {};

window.AmChartMakers.ProcessMemoryChart = {
    create: function (root, data, config) {
        // 차트 생성
        const chart = root.container.children.push(am5xy.XYChart.new(root, {
            panX: true,
            panY: true,
            wheelX: "panX",
            wheelY: "zoomX",
            pinchZoomX: true,
            // 툴팁이 여러 개 동시에 표시될 수 있도록 설정
            maxTooltipDistance: -1
        }));

        // 다크모드 여부에 따라 텍스트 색 설정
        const isDarkMode = document.body.querySelector('.dark-theme-main-content') !== null;
        const textColor = isDarkMode ? am5.color(0xffffff) : am5.color(0x000000);

        // X축 설정
        const xAxis = chart.xAxes.push(am5xy.DateAxis.new(root, {
            maxDeviation: 0.2,
            baseInterval: { timeUnit: "second", count: 1 },
            renderer: am5xy.AxisRendererX.new(root, {
                minorGridEnabled: true,
                // ▼▼▼ [핵심 수정 1/2] 라벨 간 최소 간격을 설정하여 겹침을 방지합니다. (단위: 픽셀) ▼▼▼
                minGridDistance: 120
            }),
            /*tooltip: am5.Tooltip.new(root, {})*/
        }));
        // ▼▼▼ [핵심 수정 2/2] X축 라벨의 날짜/시간 형식을 지정합니다. ▼▼▼
        const consistentFormat = "MM-dd HH:mm";
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

        xAxis.get("renderer").labels.template.setAll({ fill: textColor, rotation: -45 });

        // Y축 설정
        const yAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
            renderer: am5xy.AxisRendererY.new(root, {})
        }));
        yAxis.get("renderer").labels.template.setAll({ fill: textColor });
        yAxis.children.push(am5.Label.new(root, {
            text: "Memory (MB)", rotation: -90, y: am5.p50, centerX: am5.p50, fill: textColor
        }));

        // ▼▼▼ [핵심 수정 1/3] 구별하기 쉬운 색상 팔레트를 정의합니다. ▼▼▼
        const colors = [
            am5.color(0x33b2ff), am5.color(0x39e6a3), am5.color(0xffcb33),
            am5.color(0xff6666), am5.color(0x9966ff), am5.color(0xff9933),
            am5.color(0x00cc99), am5.color(0xff66b3)
        ];

        // 유니크 프로세스별로 Series 생성
        const uniqueProcesses = [...new Set(data.map(item => item.processName))];

        // ▼▼▼ [핵심 수정 2/3] forEach 루프에 index 파라미터를 추가합니다. ▼▼▼
        uniqueProcesses.forEach((procName, index) => {
            const series = chart.series.push(am5xy.LineSeries.new(root, {
                name: procName,
                xAxis: xAxis,
                yAxis: yAxis,
                valueYField: "memoryUsageMB",
                valueXField: "timestamp",
                legendValueText: "{valueY} MB", // 범례 값 포맷
                tooltip: am5.Tooltip.new(root, {
                    pointerOrientation: "horizontal",
                    labelText: "[bold]{name}:[/] {valueY} MB\n{valueX.formatDate('MM-dd HH:mm:ss')}"
                })
            }));

            // ▼▼▼ [핵심 수정 3/3] 정의된 색상 팔레트에서 순서대로 색상을 할당합니다. ▼▼▼
            const color = colors[index % colors.length];
            series.set("stroke", color);
            series.set("fill", color);

            series.strokes.template.set("strokeWidth", 2);

            series.bullets.push(function() {
                return am5.Bullet.new(root, {
                    sprite: am5.Circle.new(root, {
                        radius: 1.8,
                        fill: series.get("stroke"),
                        stroke: series.get("stroke"),
                        strokeWidth: 1
                    })
                });
            });

            series.data.processor = am5.DataProcessor.new(root, {
                dateFields: ["timestamp"],
                dateFormat: "yyyy-MM-ddTHH:mm:ss"
            });

            const processData = data.filter(d => d.processName === procName);
            series.data.setAll(processData);
            series.appear();
        });

        // 커서 생성 (세로선만 보이도록 설정)
        const cursor = chart.set("cursor", am5xy.XYCursor.new(root, {
            behavior: "snapX" // 마우스 움직임을 그대로 따라오도록 설정
        }));
        cursor.lineY.set("visible", false);
        cursor.lineX.set("visible", true); // 세로선 표시

        // 범례
        const legend = chart.rightAxesContainer.children.push(am5.Legend.new(root, {
            width: 200,
            paddingLeft: 15,
            height: am5.percent(100)
        }));

        // 범례 아이템에 마우스를 올렸을 때의 동작
        legend.itemContainers.template.events.on("pointerover", function (e) {
            const series = e.target.dataItem.dataContext;
            chart.series.each(function (chartSeries) {
                if (chartSeries !== series) {
                    chartSeries.strokes.template.setAll({ strokeOpacity: 0.15, stroke: am5.color(0x888888) });
                } else {
                    chartSeries.strokes.template.setAll({ strokeWidth: 4 });
                }
            });
        });

        // 범례 아이템에서 마우스를 뗐을 때의 동작
        legend.itemContainers.template.events.on("pointerout", function (e) {
            chart.series.each(function (chartSeries) {
                chartSeries.strokes.template.setAll({
                    strokeOpacity: 1,
                    strokeWidth: 2,
                    stroke: chartSeries.get("fill")
                });
            });
        });

        legend.itemContainers.template.set("width", am5.p100);
        legend.valueLabels.template.setAll({ width: am5.p100, textAlign: "right" });
        legend.data.setAll(chart.series.values);

        chart.appear(1000, 100);
    }
};
