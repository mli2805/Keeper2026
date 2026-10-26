using System;
using System.Collections.Generic;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes.Charts;

namespace KeeperWpf;

public static partial class PdfCarReportFactory
{
    private static readonly Color[] CarChartColors =
    [
        new(0xFF9999FF), new(0xFF993366), new(0xFFFFFFCC), new(0xFFCCFFFF),
        new(0xFF660066), new(0xFFFF8080), new(0xFF0066CC), new(0xFFCCCCFF),
        new(0xFF000080), new(0xFFFF00FF), new(0xFFFFFF00), new(0xFF00FFFF),
        new(0xFF800080), new(0xFF800000), new(0xFF008080), new(0xFF0000FF)
    ];

    private static void DrawChart(this Section section, CarReportData carReportData)
    {
        var chart3 = section.AddChart(ChartType.PieExploded2D);
        Series pieSeries = chart3.SeriesCollection.AddSeries();
        pieSeries.DataLabel.Type = DataLabelType.Value;
        pieSeries.DataLabel.Position = DataLabelPosition.OutsideEnd;
        pieSeries.HasDataLabel = true;
        chart3.Width = Unit.FromCentimeter(16);
        chart3.Height = Unit.FromCentimeter(16);
        XSeries pieXSeries = chart3.XValues.AddXSeries();

        var categories = carReportData.Categories.ToList();
        for (var index = 0; index < categories.Count; index++)
        {
            var tag = categories[index];
            var color = CarChartColors[index % CarChartColors.Length];
            var point = pieSeries.Add(Math.Round((double)-tag.Table.Sum(r => r.AmountInUsd)));
            point.FillFormat.Color = color;
            pieXSeries.Add(tag.Russian);
        }

        chart3.XAxis.MajorTickMark = TickMarkType.Outside;
        chart3.XAxis.Title.Caption = "X-Axis";
        chart3.YAxis.MajorTickMark = TickMarkType.Outside;
        chart3.YAxis.TickLabels.Format = "#0%";
        chart3.YAxis.TickLabels.Font.Size = 0.01f;
        chart3.YAxis.TickLabels.Font.Color = Colors.Black;
        chart3.PivotChart = true;
        chart3.HasDataLabel = true;

        section.AddTwoRowChartLegend(categories.Select(category => category.Russian).ToList());
    }

    private static void AddTwoRowChartLegend(this Section section, IReadOnlyList<string> categories)
    {
        if (categories.Count == 0)
            return;

        var legend = section.AddTable();
        legend.AddColumn(Unit.FromCentimeter(16));
        legend.Format.Font.Size = 9;

        var categoriesPerRow = (categories.Count + 1) / 2;
        for (var rowStart = 0; rowStart < categories.Count; rowStart += categoriesPerRow)
        {
            var paragraph = legend.AddRow().Cells[0].AddParagraph();
            paragraph.Format.Alignment = ParagraphAlignment.Center;

            var rowEnd = Math.Min(rowStart + categoriesPerRow, categories.Count);
            for (var index = rowStart; index < rowEnd; index++)
            {
                var marker = paragraph.AddFormattedText("■");
                marker.Font.Color = CarChartColors[index % CarChartColors.Length];
                paragraph.AddText($" {categories[index]}   ");
            }
        }
    }

    private static void DrawAggregateTable(this Section section, CarReportData carReportData)
    {
        var paragraph = section.AddParagraph();
        paragraph.AddFormattedText("Расходы по категориям");
        paragraph.Format.SpaceBefore = Unit.FromCentimeter(1);
        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.2);

        var table = section.AddTable();
        table.Style = "Table";
        table.Borders.Width = 0.25;

        var column = table.AddColumn("8cm");
        column.Format.Alignment = ParagraphAlignment.Left;
        column = table.AddColumn("5cm");
        column.Format.Alignment = ParagraphAlignment.Right;
        column.RightPadding = Unit.FromCentimeter(0.5);
        column = table.AddColumn("3cm");
        column.Format.Alignment = ParagraphAlignment.Right;

        var total = carReportData.Categories.Sum(t => t.Table.Sum(r => r.AmountInUsd));
        foreach (var tag in carReportData.Categories)
        {
            var row = table.AddRow();
            row.Cells[0].AddParagraph($"{tag.Russian}");
            var amount = tag.Table.Sum(r => r.AmountInUsd);
            row.Cells[1].AddParagraph($"$ {-amount:#,0}");
            row.Cells[2].AddParagraph($"{amount / total * 100:N} %");
        }

        var rowTotal = table.AddRow();
        rowTotal.Cells[1].AddParagraph($"$ {-total:#,0}");
        rowTotal.Format.Font.Bold = true;

    }
}
