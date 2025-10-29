// ITM.Dashboard.Web.Client/wwwroot/js/chart-modules/error-by-eqp-column-chart.js

window.AmChartMakers = window.AmChartMakers || {};

// Error Analytics 장비별 현황 (세로 막대, 회전 라벨) 차트 Maker
window.AmChartMakers.ErrorByEqpColumnChart = {
    create: function (root, data, config) {
        const chart = root.container.children.push(am5xy.XYChart.new(root, {
            // 이 차트는 간단한 요약이므로 확대/축소/이동 기능을 비활성화합니다.
            panX: false,
            panY: false,
            wheelX: "none",
            wheelY: "none",
            paddingLeft: 0,
            paddingRight: 10 // 라벨이 잘리지 않도록 우측 여백 추가
        }));

        // 다크 모드 텍스트 색상 처리
        const isDarkMode = document.body.querySelector('.dark-theme-main-content') !== null;
        const textColor = isDarkMode ? am5.color(0xffffff) : am5.color(0x000000);

        // X축 (Category) - 데모 코드 적용
        const xRenderer = am5xy.AxisRendererX.new(root, {
            minGridDistance: 30,
            minorGridEnabled: false // 보조 격자 비활성화 (더 깔끔하게)
        });

        // 핵심: 회전된 라벨 적용
        xRenderer.labels.template.setAll({
            rotation: -45,
            centerY: am5.p50,
            centerX: am5.p100,
            paddingRight: 15,
            fill: textColor // 다크 모드 적용
        });

        xRenderer.grid.template.setAll({
            location: 1
        });

        const xAxis = chart.xAxes.push(am5xy.CategoryAxis.new(root, {
            maxDeviation: 0.3,
            categoryField: config.categoryField, // Blazor C#에서 전달된 필드명
            renderer: xRenderer
            // ▼▼▼ [수정 1/2] X축의 툴팁(하얀색 박스)을 제거합니다. ▼▼▼
            // tooltip: am5.Tooltip.new(root, {}) 
            // ▲▲▲ 이 라인을 삭제하거나 주석 처리합니다. ▲▲▲
        }));

        // Y축 (Value)
        const yRenderer = am5xy.AxisRendererY.new(root, {
            strokeOpacity: 0.1
        });

        yRenderer.labels.template.set("fill", textColor); // 다크 모드 적용

        const yAxis = chart.yAxes.push(am5xy.ValueAxis.new(root, {
            maxDeviation: 0.3,
            renderer: yRenderer,
            min: 0 // 항상 0에서 시작
        }));

        // Series (Column) - 데모 코드 적용
        const series = chart.series.push(am5xy.ColumnSeries.new(root, {
            name: "Alerts",
            xAxis: xAxis,
            yAxis: yAxis,
            valueYField: config.valueField, // Blazor C#에서 전달된 필드명
            sequencedInterpolation: true,
            categoryXField: config.categoryField // Blazor C#에서 전달된 필드명
            // ▼▼▼ [수정 2/2] 툴팁 정의를 이 위치에서 제거합니다. ▼▼▼
            // tooltip: am5.Tooltip.new(root, {
            //    labelText: "{categoryX}: {valueY}건" 
            // })
            // ▲▲▲ 이 3줄을 삭제합니다. ▲▲▲
        }));

        // 막대 스타일
        series.columns.template.setAll({
            cornerRadiusTL: 5,
            cornerRadiusTR: 5,
            strokeOpacity: 0,
            cursorOverStyle: "pointer",
            tooltipText: "{categoryX}: {valueY}건"
        });

        // ▼▼▼ [추가] 막대 클릭 이벤트 핸들러 ▼▼▼
        series.columns.template.events.on("click", function (ev) {
            // C#에서 dotNetHelper를 전달했는지 확인
            if (config.dotNetHelper) {
                // 클릭된 막대의 EQPID (categoryX) 값을 가져옵니다.
                const eqpId = ev.target.dataItem.get("categoryX");
                if (eqpId) {
                    // Blazor C#의 [JSInvokable] 메서드('OnChartEqpIdClicked')를 호출합니다.
                    config.dotNetHelper.invokeMethodAsync('OnChartEqpIdClicked', eqpId);
                }
            }
        });

        // 막대 색상
        series.columns.template.adapters.add("fill", function (fill, target) {
            return chart.get("colors").getIndex(series.columns.indexOf(target));
        });

        series.columns.template.adapters.add("stroke", function (stroke, target) {
            return chart.get("colors").getIndex(series.columns.indexOf(target));
        });

        // 데이터 설정
        xAxis.data.setAll(data);
        series.data.setAll(data);

        // 애니메이션
        series.appear(1000);
        chart.appear(1000, 100);
    }
};
