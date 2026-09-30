using System.Text.Json.Serialization;
using DataGovIL.Api.Services;

namespace DataGovIL.Api.Models;

/// <summary>
/// Public API shape for a vehicle registration row (active, inactive, or permanently cancelled).
/// Serialized with ASP.NET's default camelCase names.
/// Built from <see cref="VehicleDatastoreRecord"/>, <see cref="VehicleCancelledDatastoreRecord"/>,
/// <see cref="VehicleInactiveWithoutModelCodeDatastoreRecord"/>, <see cref="VehicleSafetyDiscountDatastoreRecord"/>,
/// or <see cref="VehiclePersonalImportDatastoreRecord"/>.
/// </summary>
public class VehicleRecord
{
    /// <summary>מזהה</summary>
    public string? Id { get; set; }

    /// <summary>מספר רכב</summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>קוד תוצר</summary>
    public string? ManufacturerCode { get; set; }

    /// <summary>סוג דגם</summary>
    public string? ModelType { get; set; }

    /// <summary>תוצר</summary>
    public string? ManufacturerName { get; set; }

    /// <summary>קוד דגם</summary>
    public string? ModelCode { get; set; }

    /// <summary>דגם</summary>
    public string? ModelName { get; set; }

    /// <summary>קוד סוג רכב</summary>
    public string? VehicleTypeCode { get; set; }

    /// <summary>סוג רכב</summary>
    public string? VehicleTypeName { get; set; }

    /// <summary>רמת גימור</summary>
    public string? TrimLevel { get; set; }

    /// <summary>רמת אבזור בטיחותי</summary>
    public string? SafetyEquipmentLevel { get; set; }

    /// <summary>קבוצת זיהום</summary>
    public string? PollutionGroup { get; set; }

    /// <summary>שנת יצור</summary>
    public string? ManufactureYear { get; set; }

    /// <summary>תוצר מנוע</summary>
    public string? EngineManufacturer { get; set; }

    /// <summary>דגם מנוע</summary>
    public string? EngineModel { get; set; }

    /// <summary>מספר מנוע</summary>
    public string? EngineNumber { get; set; }

    /// <summary>משקל כולל</summary>
    public string? TotalWeight { get; set; }

    /// <summary>תאריך מבחן אחרון</summary>
    public string? LastTestDate { get; set; }

    /// <summary>תוקף</summary>
    public string? TestValidUntil { get; set; }

    /// <summary>תאריך ביטול</summary>
    public string? CancellationDate { get; set; }

    /// <summary>בעלות</summary>
    public string? OwnershipType { get; set; }

    /// <summary>מספר שילדה</summary>
    public string? ChassisNumber { get; set; }

    /// <summary>קוד צבע</summary>
    public string? ColorCode { get; set; }

    /// <summary>צבע רכב</summary>
    public string? Color { get; set; }

    /// <summary>צמיג קדמי</summary>
    public string? FrontTire { get; set; }

    /// <summary>צמיג אחורי</summary>
    public string? RearTire { get; set; }

    /// <summary>סוג דלק</summary>
    public string? FuelType { get; set; }

    /// <summary>הוראת רישום</summary>
    public string? RegistrationOrder { get; set; }

    /// <summary>מועד עליה לכביש</summary>
    public string? RoadEntryDate { get; set; }

    /// <summary>סיום אחריות יצרן משוערת: <see cref="RoadEntryDate"/> + 36 months. Null without a road entry date.</summary>
    public DateOnly? EstimatedWarrantyEndDate =>
        VehicleDepreciationCalculator.ParseYearMonth(RoadEntryDate) is DateTime entry
            ? DateOnly.FromDateTime(entry.AddMonths(36))
            : null;

    /// <summary>אחריות יצרן משוערת בתוקף. Null without a road entry date.</summary>
    public bool? EstimatedWarrantyActive =>
        EstimatedWarrantyEndDate is DateOnly end ? end >= DateOnly.FromDateTime(DateTime.Today) : null;

    /// <summary>תאריך עדכון</summary>
    public string? UpdatedDate { get; set; }

    /// <summary>סוג יבוא</summary>
    public string? ImportType { get; set; }

    /// <summary>כינוי מסחרי</summary>
    public string? CommercialName { get; set; }

    /// <summary>ביטול סופי</summary>
    public bool IsPermanentlyCancelled { get; set; }

    /// <summary>לא פעיל</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsInactive { get; set; }

    /// <summary>זכאי להנחה לאחר התקנת מערכות בטיחות</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsSafetyDiscountEligible { get; set; }

    /// <summary>תוצר ודגם</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ManufacturerModelRecord? ManufacturerModel { get; set; }

    /// <summary>ריקולים</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<VehicleRecallRecord>? Recalls { get; set; }

    /// <summary>היסטוריה טכנית והיסטוריית בעלויות. Null when neither history resource has a row.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VehicleHistoryRecord? History { get; set; }

    /// <summary>ירידת ערך</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VehicleDepreciationRecord? Depreciation { get; set; }

    /// <summary>עלות צריכת אנרגיה חודשית משוערת (ברירת המחדל של הנסועה)</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public VehicleEnergyCost? EnergyCost { get; set; }

    /// <summary>שדות נוספים</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? ExtensionData { get; set; }

    public static VehicleRecord FromDatastore(VehicleDatastoreRecord row) => new()
    {
        Id = row.Id,
        RegistrationNumber = row.RegistrationNumber,
        ManufacturerCode = row.ManufacturerCode,
        ModelType = row.ModelType,
        ManufacturerName = row.ManufacturerName,
        ModelCode = row.ModelCode,
        ModelName = row.ModelName,
        TrimLevel = row.TrimLevel,
        SafetyEquipmentLevel = row.SafetyEquipmentLevel,
        PollutionGroup = row.PollutionGroup,
        ManufactureYear = row.ManufactureYear,
        EngineModel = row.EngineModel,
        LastTestDate = row.LastTestDate,
        TestValidUntil = row.TestValidUntil,
        OwnershipType = row.OwnershipType,
        ChassisNumber = row.ChassisNumber,
        ColorCode = row.ColorCode,
        Color = row.Color,
        FrontTire = row.FrontTire,
        RearTire = row.RearTire,
        FuelType = row.FuelType,
        RegistrationOrder = row.RegistrationOrder,
        RoadEntryDate = row.RoadEntryDate,
        CommercialName = row.CommercialName,
        IsPermanentlyCancelled = false,
        ExtensionData = row.ExtensionData
    };

    public static VehicleRecord FromCancelled(VehicleCancelledDatastoreRecord row) => new()
    {
        Id = row.Id,
        RegistrationNumber = row.RegistrationNumber,
        ManufacturerCode = row.ManufacturerCode,
        ManufacturerName = row.ManufacturerName,
        ModelCode = row.ModelCode,
        ModelName = row.ModelName,
        VehicleTypeCode = row.VehicleTypeCode,
        VehicleTypeName = row.VehicleTypeName,
        TrimLevel = row.TrimLevel,
        SafetyEquipmentLevel = row.SafetyEquipmentLevel,
        PollutionGroup = row.PollutionGroup,
        ManufactureYear = row.ManufactureYear,
        EngineManufacturer = row.EngineManufacturer,
        EngineModel = row.EngineModel,
        EngineNumber = row.EngineNumber,
        TotalWeight = row.TotalWeight,
        CancellationDate = row.CancellationDate,
        OwnershipType = row.OwnershipType,
        ChassisNumber = row.ChassisNumber,
        Color = row.Color,
        FrontTire = row.FrontTire,
        RearTire = row.RearTire,
        FuelType = row.FuelType,
        RegistrationOrder = row.RegistrationOrder,
        RoadEntryDate = row.RoadEntryDate,
        CommercialName = row.CommercialName,
        IsPermanentlyCancelled = true,
        ExtensionData = row.ExtensionData
    };

    public static VehicleRecord FromInactiveWithoutModelCode(VehicleInactiveWithoutModelCodeDatastoreRecord row) => new()
    {
        Id = row.Id,
        RegistrationNumber = row.RegistrationNumber,
        ManufacturerCode = row.ManufacturerCode,
        ManufacturerName = row.ManufacturerName,
        ModelName = row.ModelName,
        ManufactureYear = row.ManufactureYear,
        EngineModel = row.EngineModel,
        TotalWeight = row.TotalWeight,
        ChassisNumber = row.ChassisNumber,
        FuelType = row.FuelType,
        RegistrationOrder = row.RegistrationOrder,
        IsInactive = true,
        ManufacturerModel = new ManufacturerModelRecord
        {
            ManufacturerCode = row.ManufacturerCode,
            ManufacturerName = row.ManufacturerName,
            ManufacturerCountryName = row.ManufacturerCountryName,
            ModelName = row.ModelName,
            ModelYear = row.ManufactureYear,
            FuelCode = row.FuelCode,
            FuelName = row.FuelType,
            EngineDisplacement = row.EngineDisplacement,
            TotalWeight = row.TotalWeight,
            CurbWeight = row.CurbWeight,
            LiftingLoadWeight = row.LiftingLoadWeight,
            DriveCode = row.DriveCode,
            DriveName = row.DriveName,
            EuTypeApproval = row.EuTypeApproval
        }
    };

    public static VehicleRecord FromPersonalImport(VehiclePersonalImportDatastoreRecord row) => new()
    {
        Id = row.Id,
        RegistrationNumber = row.RegistrationNumber,
        ManufacturerCode = row.ManufacturerCode,
        ManufacturerName = row.ManufacturerName,
        ModelName = row.ModelName,
        VehicleTypeCode = row.VehicleTypeCode,
        VehicleTypeName = row.VehicleTypeName,
        ManufactureYear = row.ManufactureYear,
        EngineModel = row.EngineModel,
        TotalWeight = row.TotalWeight,
        LastTestDate = row.LastTestDate,
        TestValidUntil = row.TestValidUntil,
        ChassisNumber = row.ChassisNumber,
        FuelType = row.FuelType,
        RoadEntryDate = row.RoadEntryDate,
        ImportType = row.ImportType,
        ManufacturerModel = new ManufacturerModelRecord
        {
            ManufacturerCode = row.ManufacturerCode,
            ManufacturerName = row.ManufacturerName,
            ManufacturerCountryName = row.ManufacturerCountryName,
            ModelName = row.ModelName,
            ModelYear = row.ManufactureYear,
            FuelName = row.FuelType,
            EngineDisplacement = row.EngineDisplacement,
            TotalWeight = row.TotalWeight
        }
    };

    public static VehicleRecord FromSafetyDiscount(VehicleSafetyDiscountDatastoreRecord row) => new()
    {
        Id = row.Id,
        RegistrationNumber = row.RegistrationNumber,
        UpdatedDate = row.UpdatedDate,
        IsSafetyDiscountEligible = true
    };
}
