using course_project.Models;
using course_project.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace course_project.Forms
{
    public partial class ManagerStatisticsForm : Form
    {
        private readonly SaleService _saleService;
        private readonly ProductService _productService;

        // Определяем стилистику проекта
        private readonly Color _headerColor = Color.FromArgb(0, 122, 204);
        private readonly Color _backgroundColor = Color.FromArgb(240, 240, 240);
        private readonly Color _accentColor = Color.FromArgb(75, 172, 198);
        private readonly Color _textColor = Color.FromArgb(64, 64, 64);

        public ManagerStatisticsForm()
        {
            InitializeComponent();
            _saleService = new SaleService();
            _productService = new ProductService();
        }

        private void StatisticsForm_Load(object sender, EventArgs e)
        {
            this.BackColor = _backgroundColor;
            panelCharts.BackColor = _backgroundColor;
            panelCharts.Padding = new Padding(20, 0, 20, 10);

            panelCharts.Controls.Clear();

            SetupStyledLayout();
        }

        private void SetupStyledLayout()
        {
            var sales = _saleService.GetAllSales();

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                BackColor = _backgroundColor
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // *** Увеличили высоту шапки и KPI, чтобы ничего не резалось
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));  // шапка
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 160F));  // KPI
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var headerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = _headerColor,
                Margin = new Padding(0)
            };

            var titleLabel = new Label
            {
                Text = "Статистика продаж",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold), // *** немного уменьшили шрифт
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            headerPanel.Controls.Add(titleLabel);
            mainLayout.Controls.Add(headerPanel, 0, 0);
            mainLayout.SetColumnSpan(headerPanel, 2);

            if (!sales.Any())
            {
                var noDataLabel = new Label
                {
                    Text = "Нет данных для отображения.",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 14F)
                };
                mainLayout.Controls.Add(noDataLabel, 0, 1);
                mainLayout.SetColumnSpan(noDataLabel, 2);
                panelCharts.Controls.Add(mainLayout);
                return;
            }

            DisplayKPIs(sales, mainLayout);

            var chartRevenue = CreateRevenueChart(sales);
            var chartProducts = CreateTopProductsChart(sales);
            var chartMonthly = CreateMonthlySalesChart(sales);
            var chartSellers = CreateSellersChart(sales);

            mainLayout.Controls.Add(chartRevenue, 0, 2);
            mainLayout.Controls.Add(chartProducts, 1, 2);
            mainLayout.Controls.Add(chartMonthly, 0, 3);
            mainLayout.Controls.Add(chartSellers, 1, 3);

            panelCharts.Controls.Add(mainLayout);
        }

        private void DisplayKPIs(List<Sale> sales, TableLayoutPanel mainLayout)
        {
            var kpiLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(0, 5, 0, 5)
            };
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

            decimal totalRevenue = sales.Sum(s => s.TotalAmount);
            int salesCount = sales.Count;
            decimal averageCheck = salesCount > 0 ? totalRevenue / salesCount : 0;

            kpiLayout.Controls.Add(CreateKpiCard("Общая выручка", $"{totalRevenue:N0} ₽"), 0, 0);
            kpiLayout.Controls.Add(CreateKpiCard("Всего продаж", $"{salesCount}"), 1, 0);
            kpiLayout.Controls.Add(CreateKpiCard("Средний чек", $"{averageCheck:N0} ₽"), 2, 0);

            mainLayout.Controls.Add(kpiLayout, 0, 1);
            mainLayout.SetColumnSpan(kpiLayout, 2);
        }

        private Panel CreateKpiCard(string title, string value)
        {
            var panel = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Margin = new Padding(5, 0, 5, 0),
                Padding = new Padding(5)
            };

            var textLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White
            };

            // *** 50/50 по высоте, чтобы и заголовок и число имели место
            textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            textLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Dock = DockStyle.Fill,        // *** Fill вместо Top
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            var lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold), // *** можно 18–22 подбирать
                ForeColor = _textColor,
                Dock = DockStyle.Fill,        // *** Fill вместо Top
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            textLayout.Controls.Add(lblTitle, 0, 0);
            textLayout.Controls.Add(lblValue, 0, 1);

            panel.Controls.Add(textLayout);

            return panel;
        }

        private GroupBox CreateChartGroupBox(Chart chart, string title)
        {
            var groupBox = new GroupBox
            {
                Text = title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = _textColor,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 5, 10, 10),
                Margin = new Padding(10)
            };
            groupBox.Controls.Add(chart);
            return groupBox;
        }

        private Chart CreateBaseChart()
        {
            Chart chart = new Chart { Dock = DockStyle.Fill };
            ChartArea area = new ChartArea { BackColor = Color.White };
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8F);
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8F);
            area.AxisY.LabelStyle.Format = "N0";
            chart.ChartAreas.Add(area);
            return chart;
        }

        private GroupBox CreateRevenueChart(List<Sale> sales)
        {
            Chart chart = CreateBaseChart();
            var series = new Series("Выручка")
            {
                ChartType = SeriesChartType.Column,
                Color = _accentColor,
                IsValueShownAsLabel = true,
                LabelFormat = "N0 ₽"
            };
            var data = sales.GroupBy(s => s.SaleDate.Date).OrderBy(g => g.Key).TakeLast(10)
                            .Select(g => new { Date = g.Key.ToString("dd.MM"), Total = g.Sum(s => s.TotalAmount) });
            series.Points.DataBind(data, "Date", "Total", "");
            chart.Series.Add(series);
            return CreateChartGroupBox(chart, "Выручка за последние 10 дней");
        }

        private GroupBox CreateTopProductsChart(List<Sale> sales)
        {
            Chart chart = CreateBaseChart();
            chart.Legends.Add(new Legend("ProductsLegend")
            {
                Docking = Docking.Bottom,
                Font = new Font("Segoe UI", 8F),
                Alignment = StringAlignment.Near
            });
            var series = new Series("Товары")
            {
                ChartType = SeriesChartType.Doughnut,
                Label = "#PERCENT{P0}",
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                LabelForeColor = Color.White
            };

            var palette = new Color[]
            {
                Color.FromArgb(0, 122, 204),
                Color.FromArgb(255, 187, 0),
                Color.FromArgb(220, 53, 69),
                Color.FromArgb(25, 135, 84),
                Color.FromArgb(108, 117, 125)
            };
            chart.Palette = ChartColorPalette.None;
            chart.PaletteCustomColors = palette;

            var data = sales.SelectMany(s => s.Items).GroupBy(i => i.ProductId)
                            .Select(g => new
                            {
                                Name = _productService.GetProductById(g.Key)?.ProductName ?? "N/A",
                                Qty = g.Sum(i => i.Quantity)
                            })
                            .OrderByDescending(x => x.Qty).Take(5).ToList();
            foreach (var item in data)
            {
                var p = series.Points.Add(item.Qty);
                p.LegendText = $"{item.Name} ({item.Qty} шт.)";
            }
            chart.Series.Add(series);
            return CreateChartGroupBox(chart, "Топ-5 продаваемых товаров (по количеству)");
        }

        private GroupBox CreateMonthlySalesChart(List<Sale> sales)
        {
            Chart chart = CreateBaseChart();
            var series = new Series("Объем продаж")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(220, 53, 69),
                BorderWidth = 3,
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 8
            };
            var data = sales.GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                            .Select(g => new
                            {
                                Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yy"),
                                Total = g.Sum(s => s.TotalAmount)
                            });
            series.Points.DataBind(data, "Month", "Total", "");
            chart.Series.Add(series);
            return CreateChartGroupBox(chart, "Динамика продаж по месяцам");
        }

        private GroupBox CreateSellersChart(List<Sale> sales)
        {
            Chart chart = CreateBaseChart();
            chart.ChartAreas[0].AxisX.Interval = 1;
            var series = new Series("Продавцы")
            {
                ChartType = SeriesChartType.Bar,
                IsValueShownAsLabel = true,
                LabelFormat = "N0 ₽",
                Color = _accentColor
            };
            var data = sales.GroupBy(s => s.SellerUserName)
                            .Select(g => new { Seller = g.Key, Total = g.Sum(s => s.TotalAmount) })
                            .OrderBy(x => x.Total);
            series.Points.DataBind(data, "Seller", "Total", "");
            chart.Series.Add(series);
            return CreateChartGroupBox(chart, "Эффективность продавцов");
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
            ManagerMenuForm menu = new ManagerMenuForm();
            menu.Show();
        }
    }
}
