using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace KeeperWpf;

public static partial class PdfCarReportFactory
{
    public static PdfDocument CreateCarReport(this KeeperDataModel dataModel,
        int carId, bool isByTags, bool isBynInReport)
    {
        var car = dataModel.Cars.First(c => c.Id == carId);
        var carReportData = dataModel.ExtractCarReportData(car, isByTags);

        Document doc = new Document();

        Section firstPageSection = doc.AddSection();
        firstPageSection.PageSetup.LeftMargin = Unit.FromCentimeter(1.0);
        firstPageSection.PageSetup.TopMargin = 40;
        firstPageSection.PageSetup.BottomMargin = 10;
        var paragraph = firstPageSection.AddParagraph();
        paragraph.AddFormattedText($"{car.Title} {car.IssueYear} г.в. {car.PurchaseDate:dd/MM/yyyy} - {car.SaleDate:dd/MM/yyyy}");
        paragraph.Format.SpaceAfter = Unit.FromCentimeter(0.3);

        firstPageSection.DrawChart(carReportData);
        firstPageSection.DrawAggregateTable(carReportData);
        firstPageSection.DrawTotals(carReportData, car, dataModel);

        Section tablesSection = doc.AddSection();
        tablesSection.PageSetup.LeftMargin = Unit.FromCentimeter(1.0);
        tablesSection.PageSetup.TopMargin = 20;
        tablesSection.PageSetup.BottomMargin = 10;
        tablesSection.DrawTagTables(carReportData, isByTags, isBynInReport);

        var pdfDocumentRenderer = new PdfDocumentRenderer() { Document = doc };
        GlobalFontSettings.FontResolver = new EmbeddedFontResolver();
        pdfDocumentRenderer.RenderDocument();

        return pdfDocumentRenderer.PdfDocument;
    }
}
