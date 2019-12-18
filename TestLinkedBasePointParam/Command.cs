#region Namespaces
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace TestLinkedBasePointParam
{
  /// <summary>
  /// Test access to base point parameter values in 
  /// linked document for SFDC case 16069173 [Revit 
  /// 2020.1 update broke the API].
  /// </summary>
  [Transaction( TransactionMode.ReadOnly )]
  public class Command : IExternalCommand
  {
    /// <summary>
    /// Return a string for a real number
    /// formatted to two decimal places.
    /// </summary>
    public static string RealString( double a )
    {
      return a.ToString( "0.##" );
    }

    /// <summary>
    /// Return a string for an XYZ point
    /// or vector with its coordinates
    /// formatted to two decimal places.
    /// </summary>
    public static string PointString(
      XYZ p,
      bool onlySpaceSeparator = false )
    {
      string format_string = onlySpaceSeparator
        ? "{0} {1} {2}"
        : "({0},{1},{2})";

      return string.Format( format_string,
        RealString( p.X ),
        RealString( p.Y ),
        RealString( p.Z ) );
    }

    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements )
    {
      UIApplication uiapp = commandData.Application;
      UIDocument uidoc = uiapp.ActiveUIDocument;
      Document doc = uidoc.Document;

      FilteredElementCollector links
        = new FilteredElementCollector( doc )
          .WhereElementIsNotElementType()
          .OfCategory( BuiltInCategory.OST_RvtLinks );

      foreach( RevitLinkInstance i in links )
      {
        Debug.Print( i.Name );
        Document doc_linked = i.GetLinkDocument();

        FilteredElementCollector basepoints
          = new FilteredElementCollector( doc )
            .WhereElementIsNotElementType()
            .OfCategory( BuiltInCategory.OST_ProjectBasePoint );

        foreach( BasePoint bp in basepoints )
        {
          double x = bp.get_Parameter( BuiltInParameter.BASEPOINT_EASTWEST_PARAM ).AsDouble();
          double y = bp.get_Parameter( BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM ).AsDouble();
          double z = bp.get_Parameter( BuiltInParameter.BASEPOINT_ELEVATION_PARAM ).AsDouble();
          XYZ p = new XYZ( x, y, z );
          Debug.Print( PointString( p ) );
        }
      }

      /*
      Dim MyDoc As Document
      MyDoc = Revit.Application.ActiveUIDocument.Document
      Dim LinkInstanceCollector As New FilteredElementCollector(MyDoc)
      Dim LinkedInstances As IList(Of Element) = LinkInstanceCollector.OfCategory(BuiltInCategory.OST_RvtLinks).OfClass(GetType(RevitLinkInstance)).ToElements()
      For Each currentLinkElement As Element In LinkedInstances
      Try
      Dim CurrentlinkInstance As RevitLinkInstance = TryCast(currentLinkElement, RevitLinkInstance)
      If CurrentlinkInstance IsNot Nothing Then
      Dim LinkProjectBasePoint As XYZ = XYZ.Zero
      Dim TempLinkDoc As Document = CurrentlinkInstance.GetLinkDocument
      If TempLinkDoc IsNot Nothing Then
      Dim LinkBasePointCollector As New FilteredElementCollector(TempLinkDoc)
      Dim LinkedBasePointElems As IList(Of Element) = LinkBasePointCollector.OfCategory(BuiltInCategory.OST_ProjectBasePoint).WhereElementIsNotElementType.ToElements()
      For Each CurrentBasepointElement As Element In LinkedBasePointElems
      Dim CurrentBasePoint As BasePoint = TryCast(CurrentBasepointElement, BasePoint)
      If CurrentBasePoint IsNot Nothing Then
      'Revit 2020.1 broke the linked project base point parameters
      LinkProjectBasePoint = New XYZ(
      CurrentBasePoint.Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble,
      CurrentBasePoint.Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble, 
      CurrentBasePoint.Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble)
      Exit For
      End If
      Next
      End If
      End If
      Catch ex As Exception
      End Try
      Next
      */

      return Result.Succeeded;
    }
  }
}
