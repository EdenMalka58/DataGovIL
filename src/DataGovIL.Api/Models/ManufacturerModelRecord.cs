using System.Globalization;
using System.Text.Json.Serialization;
using DataGovIL.Api.Data;
using DataGovIL.Api.Services;

namespace DataGovIL.Api.Models;

/// <summary>
/// Public API shape for a WLTP make/model row.
/// Serialized with ASP.NET's default camelCase names (<c>manufacturerName</c>, not <c>tozeret_nm</c>).
/// Built from <see cref="ManufacturerModelDatastoreRecord"/> after CKAN deserialization.
/// </summary>
public class ManufacturerModelRecord
{
    /// <summary>מזהה</summary>
    public string? Id { get; set; }

    /// <summary>סוג דגם</summary>
    public string? ModelType { get; set; }

    /// <summary>קוד תוצר</summary>
    public string? ManufacturerCode { get; set; }

    /// <summary>תוצר</summary>
    public string? ManufacturerName { get; set; }

    /// <summary>
    /// Motomarks logo slug for <see cref="ManufacturerCode"/> when that maker has a published logo.
    /// Null when the code is missing from the logo catalog or marked as having no logo.
    /// </summary>
    public string? LogoSlug => MotomarksLogoCatalog.SlugFor(ManufacturerCode);

    /// <summary>ארץ תוצר</summary>
    public string? ManufacturerCountryName { get; set; }

    /// <summary>תוצר</summary>
    public string? Tozar { get; set; }

    /// <summary>קוד דגם</summary>
    public string? ModelCode { get; set; }

    /// <summary>דגם</summary>
    public string? ModelName { get; set; }

    /// <summary>שנת יצור</summary>
    public string? ModelYear { get; set; }

    /// <summary>קבוצת אגרה</summary>
    public string? FeeGroupCode { get; set; }

    /// <summary>שווי שימוש חודשי משוער (₪), derived from <see cref="FeeGroupCode"/>.</summary>
    public decimal? FeeGroupPrice => FeeGroupUsageValue.ForFeeGroup(FeeGroupCode);

    /// <summary>
    /// אגרת רישוי משוערת (₪) for the current fee year, from <see cref="FeeGroupCode"/> and the vehicle's
    /// first registration date. Set only on vehicle lookups; null on catalog rows, which have no registration date.
    /// </summary>
    public decimal? EstimatedLicenseFee { get; set; }

    /// <summary>נפח מנוע</summary>
    public string? EngineDisplacement { get; set; }

    /// <summary>משקל כולל</summary>
    public string? TotalWeight { get; set; }

    /// <summary>משקל עצמי</summary>
    public string? CurbWeight { get; set; }

    /// <summary>משקל מטען הרמה</summary>
    public string? LiftingLoadWeight { get; set; }

    /// <summary>גובה</summary>
    public string? Height { get; set; }

    /// <summary>קוד הנעה</summary>
    public string? DriveCode { get; set; }

    /// <summary>הנעה</summary>
    public string? DriveName { get; set; }

    /// <summary>מזגן</summary>
    public string? AirConditioningIndicator { get; set; }

    /// <summary>ABS</summary>
    public string? AbsIndicator { get; set; }

    /// <summary>מקור כריות אוויר</summary>
    public string? AirbagsSource { get; set; }

    /// <summary>מספר כריות אוויר</summary>
    public string? AirbagCount { get; set; }

    /// <summary>הגה כוח</summary>
    public string? PowerSteeringIndicator { get; set; }

    /// <summary>תיבת הילוכים אוטומטית</summary>
    public string? AutomaticTransmissionIndicator { get; set; }

    /// <summary>מקור חלונות חשמל</summary>
    public string? PowerWindowsSource { get; set; }

    /// <summary>מספר חלונות חשמל</summary>
    public string? PowerWindowCount { get; set; }

    /// <summary>חלון בגג</summary>
    public string? PowerSunroofIndicator { get; set; }

    /// <summary>גלגלי סגסוגת קלה</summary>
    public string? AlloyWheelsIndicator { get; set; }

    /// <summary>ארגז</summary>
    public string? CargoBoxIndicator { get; set; }

    /// <summary>מרכב</summary>
    public string? BodyType { get; set; }

    /// <summary>רמת גימור</summary>
    public string? TrimLevel { get; set; }

    /// <summary>קוד דלק</summary>
    public string? FuelCode { get; set; }

    /// <summary>סוג דלק</summary>
    public string? FuelName { get; set; }

    /// <summary>מספר דלתות</summary>
    public string? DoorCount { get; set; }

    /// <summary>כוח סוס</summary>
    public string? Horsepower { get; set; }

    /// <summary>מספר מושבים</summary>
    public string? SeatCount { get; set; }

    /// <summary>בקרת יציבות</summary>
    public string? StabilityControlIndicator { get; set; }

    /// <summary>כושר גרירה עם בלמים</summary>
    public string? TowingCapacityWithBrakes { get; set; }

    /// <summary>כושר גרירה בלי בלמים</summary>
    public string? TowingCapacityWithoutBrakes { get; set; }

    /// <summary>קוד סוג תקינה</summary>
    public string? HomologationTypeCode { get; set; }

    /// <summary>סוג תקינה</summary>
    public string? HomologationTypeName { get; set; }

    /// <summary>תקינה EU</summary>
    public string? EuTypeApproval { get; set; }

    /// <summary>קוד סוג ממיר</summary>
    public string? ConverterTypeCode { get; set; }

    /// <summary>סוג ממיר</summary>
    public string? ConverterTypeName { get; set; }

    /// <summary>קוד טכנולוגיית הנעה</summary>
    public string? DriveTechnologyCode { get; set; }

    /// <summary>טכנולוגיית הנעה</summary>
    public string? DriveTechnologyName { get; set; }

    /// <summary>כמות CO2</summary>
    public string? Co2Amount { get; set; }

    /// <summary>כמות NOX</summary>
    public string? NoxAmount { get; set; }

    /// <summary>כמות PM10</summary>
    public string? Pm10Amount { get; set; }

    /// <summary>כמות HC</summary>
    public string? HcAmount { get; set; }

    /// <summary>כמות HC+NOX</summary>
    public string? HcNoxAmount { get; set; }

    /// <summary>כמות CO</summary>
    public string? CoAmount { get; set; }

    /// <summary>כמות CO2 בעיר</summary>
    public string? Co2AmountCity { get; set; }

    /// <summary>כמות NOX בעיר</summary>
    public string? NoxAmountCity { get; set; }

    /// <summary>כמות PM10 בעיר</summary>
    public string? Pm10AmountCity { get; set; }

    /// <summary>כמות HC בעיר</summary>
    public string? HcAmountCity { get; set; }

    /// <summary>כמות CO בעיר</summary>
    public string? CoAmountCity { get; set; }

    /// <summary>כמות CO2 בבינעירוני</summary>
    public string? Co2AmountHighway { get; set; }

    /// <summary>כמות NOX בבינעירוני</summary>
    public string? NoxAmountHighway { get; set; }

    /// <summary>כמות PM10 בבינעירוני</summary>
    public string? Pm10AmountHighway { get; set; }

    /// <summary>כמות HC בבינעירוני</summary>
    public string? HcAmountHighway { get; set; }

    /// <summary>כמות CO בבינעירוני</summary>
    public string? CoAmountHighway { get; set; }

    /// <summary>מדד ירוק</summary>
    public string? GreenScore { get; set; }

    /// <summary>קבוצת זיהום</summary>
    public string? PollutionGroup { get; set; }

    /// <summary>בקרת סטייה מנתיב</summary>
    public string? LaneDepartureControlIndicator { get; set; }

    /// <summary>מקור התקנה בקרת סטייה מנתיב</summary>
    public string? LaneDepartureControlRegulationSource { get; set; }

    /// <summary>ניטור מרחק מלפנים</summary>
    public string? ForwardDistanceMonitoringIndicator { get; set; }

    /// <summary>מקור התקנה ניטור מרחק מלפנים</summary>
    public string? ForwardDistanceMonitoringRegulationSource { get; set; }

    /// <summary>זיהוי בשטח נסתר</summary>
    public string? BlindSpotDetectionIndicator { get; set; }

    /// <summary>בקרת שיוט אדפטיבית</summary>
    public string? AdaptiveCruiseControlIndicator { get; set; }

    /// <summary>זיהוי הולכי רגל</summary>
    public string? PedestrianDetectionIndicator { get; set; }

    /// <summary>מקור התקנה זיהוי הולכי רגל</summary>
    public string? PedestrianDetectionRegulationSource { get; set; }

    /// <summary>מערכת עזר לבלימה</summary>
    public string? BrakeAssistIndicator { get; set; }

    /// <summary>מצלמת רוורס</summary>
    public string? ReverseCameraIndicator { get; set; }

    /// <summary>חיישני לחץ אוויר בצמיגים</summary>
    public string? TirePressureMonitoringIndicator { get; set; }

    /// <summary>חיישני חגורות בטיחות</summary>
    public string? SeatbeltReminderIndicator { get; set; }

    /// <summary>ניקוד בטיחות</summary>
    public string? SafetyScore { get; set; }

    /// <summary>רמת אבזור בטיחותי</summary>
    public string? SafetyEquipmentLevel { get; set; }

    /// <summary>תאורה אוטומטית בנסיעה קדימה</summary>
    public string? AutomaticLightingForwardTravelIndicator { get; set; }

    /// <summary>שליטה אוטומטית באורות גבוהים</summary>
    public string? AutomaticHighBeamControlIndicator { get; set; }

    /// <summary>מקור התקנה שליטה אוטומטית באורות גבוהים</summary>
    public string? AutomaticHighBeamControlRegulationSource { get; set; }

    /// <summary>זיהוי מצב התקרבות מסוכנת</summary>
    public string? DangerousApproachDetectionIndicator { get; set; }

    /// <summary>זיהוי תמרורי תנועה</summary>
    public string? TrafficSignRecognitionIndicator { get; set; }

    /// <summary>זיהוי רכב דו גלגלי</summary>
    public string? TwoWheeledVehicleDetection { get; set; }

    /// <summary>מקור התקנה זיהוי תמרורי תנועה</summary>
    public string? TrafficSignRecognitionRegulationSource { get; set; }

    /// <summary>CO2 WLTP</summary>
    public string? Co2Wltp { get; set; }

    /// <summary>HC WLTP</summary>
    public string? HcWltp { get; set; }

    /// <summary>PM WLTP</summary>
    public string? PmWltp { get; set; }

    /// <summary>NOX WLTP</summary>
    public string? NoxWltp { get; set; }

    /// <summary>CO WLTP</summary>
    public string? CoWltp { get; set; }

    /// <summary>CO2 WLTP NEDC</summary>
    public string? Co2WltpNedc { get; set; }

    /// <summary>בקרת סטייה אקטיבית</summary>
    public string? ActiveLaneControl { get; set; }

    /// <summary>בלימה אוטומטית בנסיעה לאחור</summary>
    public string? AutomaticBrakingReverseTravel { get; set; }

    /// <summary>בקרת מהירות ISA</summary>
    public string? SpeedControlIsa { get; set; }

    /// <summary>בלימת חירום לפני הולכי רגל ואופניים</summary>
    public string? EmergencyBrakingPedestriansCyclists { get; set; }

    /// <summary>התנגשות צד שטח מת</summary>
    public string? SideCollisionBlindSpot { get; set; }

    /// <summary>אלכו-לוק</summary>
    public string? AlcoLock { get; set; }

    /// <summary>דרגת מתח סוללה</summary>
    public string? BatteryVoltageDg { get; set; }

    /// <summary>כינוי מסחרי</summary>
    public string? CommercialName { get; set; }

    /// <summary>שדות נוספים</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? ExtensionData { get; set; }

    public static ManufacturerModelRecord FromDatastore(ManufacturerModelDatastoreRecord row) => new()
    {
        Id = row.Id,
        ModelType = row.ModelType,
        ManufacturerCode = row.ManufacturerCode,
        ManufacturerName = row.ManufacturerName,
        ManufacturerCountryName = row.ManufacturerCountryName,
        Tozar = row.Tozar,
        ModelCode = row.ModelCode,
        ModelName = row.ModelName,
        ModelYear = row.ModelYear,
        FeeGroupCode = row.FeeGroupCode,
        EngineDisplacement = row.EngineDisplacement,
        TotalWeight = row.TotalWeight,
        Height = row.Height,
        DriveCode = row.DriveCode,
        DriveName = row.DriveName,
        AirConditioningIndicator = row.AirConditioningIndicator,
        AbsIndicator = row.AbsIndicator,
        AirbagsSource = row.AirbagsSource,
        AirbagCount = row.AirbagCount,
        PowerSteeringIndicator = row.PowerSteeringIndicator,
        AutomaticTransmissionIndicator = row.AutomaticTransmissionIndicator,
        PowerWindowsSource = row.PowerWindowsSource,
        PowerWindowCount = row.PowerWindowCount,
        PowerSunroofIndicator = row.PowerSunroofIndicator,
        AlloyWheelsIndicator = row.AlloyWheelsIndicator,
        CargoBoxIndicator = row.CargoBoxIndicator,
        BodyType = row.BodyType,
        TrimLevel = row.TrimLevel,
        FuelCode = row.FuelCode,
        FuelName = row.FuelName,
        DoorCount = row.DoorCount,
        Horsepower = row.Horsepower,
        SeatCount = row.SeatCount,
        StabilityControlIndicator = row.StabilityControlIndicator,
        TowingCapacityWithBrakes = row.TowingCapacityWithBrakes,
        TowingCapacityWithoutBrakes = row.TowingCapacityWithoutBrakes,
        HomologationTypeCode = row.HomologationTypeCode,
        HomologationTypeName = row.HomologationTypeName,
        ConverterTypeCode = row.ConverterTypeCode,
        ConverterTypeName = row.ConverterTypeName,
        DriveTechnologyCode = row.DriveTechnologyCode,
        DriveTechnologyName = row.DriveTechnologyName,
        Co2Amount = row.Co2Amount,
        NoxAmount = row.NoxAmount,
        Pm10Amount = row.Pm10Amount,
        HcAmount = row.HcAmount,
        HcNoxAmount = row.HcNoxAmount,
        CoAmount = row.CoAmount,
        Co2AmountCity = row.Co2AmountCity,
        NoxAmountCity = row.NoxAmountCity,
        Pm10AmountCity = row.Pm10AmountCity,
        HcAmountCity = row.HcAmountCity,
        CoAmountCity = row.CoAmountCity,
        Co2AmountHighway = row.Co2AmountHighway,
        NoxAmountHighway = row.NoxAmountHighway,
        Pm10AmountHighway = row.Pm10AmountHighway,
        HcAmountHighway = row.HcAmountHighway,
        CoAmountHighway = row.CoAmountHighway,
        GreenScore = row.GreenScore,
        PollutionGroup = row.PollutionGroup,
        LaneDepartureControlIndicator = row.LaneDepartureControlIndicator,
        LaneDepartureControlRegulationSource = row.LaneDepartureControlRegulationSource,
        ForwardDistanceMonitoringIndicator = row.ForwardDistanceMonitoringIndicator,
        ForwardDistanceMonitoringRegulationSource = row.ForwardDistanceMonitoringRegulationSource,
        BlindSpotDetectionIndicator = row.BlindSpotDetectionIndicator,
        AdaptiveCruiseControlIndicator = row.AdaptiveCruiseControlIndicator,
        PedestrianDetectionIndicator = row.PedestrianDetectionIndicator,
        PedestrianDetectionRegulationSource = row.PedestrianDetectionRegulationSource,
        BrakeAssistIndicator = row.BrakeAssistIndicator,
        ReverseCameraIndicator = row.ReverseCameraIndicator,
        TirePressureMonitoringIndicator = row.TirePressureMonitoringIndicator,
        SeatbeltReminderIndicator = row.SeatbeltReminderIndicator,
        SafetyScore = row.SafetyScore,
        SafetyEquipmentLevel = row.SafetyEquipmentLevel,
        AutomaticLightingForwardTravelIndicator = row.AutomaticLightingForwardTravelIndicator,
        AutomaticHighBeamControlIndicator = row.AutomaticHighBeamControlIndicator,
        AutomaticHighBeamControlRegulationSource = row.AutomaticHighBeamControlRegulationSource,
        DangerousApproachDetectionIndicator = row.DangerousApproachDetectionIndicator,
        TrafficSignRecognitionIndicator = row.TrafficSignRecognitionIndicator,
        TwoWheeledVehicleDetection = row.TwoWheeledVehicleDetection,
        TrafficSignRecognitionRegulationSource = row.TrafficSignRecognitionRegulationSource,
        Co2Wltp = row.Co2Wltp,
        HcWltp = row.HcWltp,
        PmWltp = row.PmWltp,
        NoxWltp = row.NoxWltp,
        CoWltp = row.CoWltp,
        Co2WltpNedc = row.Co2WltpNedc,
        ActiveLaneControl = row.ActiveLaneControl,
        AutomaticBrakingReverseTravel = row.AutomaticBrakingReverseTravel,
        SpeedControlIsa = row.SpeedControlIsa,
        EmergencyBrakingPedestriansCyclists = row.EmergencyBrakingPedestriansCyclists,
        SideCollisionBlindSpot = row.SideCollisionBlindSpot,
        AlcoLock = row.AlcoLock,
        BatteryVoltageDg = row.BatteryVoltageDg,
        CommercialName = row.CommercialName,
        ExtensionData = row.ExtensionData
    };

    /// <summary>
    /// Maps a stored catalog row back to the public shape. Display names for coded columns
    /// come from <paramref name="texts"/> (keyed by texts table id + id). Manufacturer name,
    /// brand, and country come from <paramref name="manufacturer"/> when that code exists.
    /// </summary>
    public static ManufacturerModelRecord FromEntity(
        ManufacturerModelEntity row,
        ManufacturerEntity? manufacturer,
        IReadOnlyDictionary<(int TableId, int Id), string> texts) => new()
    {
        ModelType = row.ModelType,
        ManufacturerCode = row.ManufacturerCode.ToString(CultureInfo.InvariantCulture),
        ManufacturerName = manufacturer?.ManufacturerName,
        ManufacturerCountryName = manufacturer?.ManufacturerCountry,
        Tozar = manufacturer?.Brand,
        ModelCode = row.ModelCode.ToString(CultureInfo.InvariantCulture),
        ModelName = row.ModelName,
        ModelYear = row.ModelYear.ToString(CultureInfo.InvariantCulture),
        FeeGroupCode = Decimal(row.FeeGroupCode),
        EngineDisplacement = Decimal(row.EngineDisplacement),
        TotalWeight = Decimal(row.TotalWeight),
        Height = Decimal(row.Height),
        DriveCode = Code(row.DriveCode),
        DriveName = Text(texts, TextTable.Drive, row.DriveCode),
        AirConditioningIndicator = Decimal(row.AirConditioningInd),
        AbsIndicator = Decimal(row.AbsInd),
        AirbagsSource = row.AirbagsSource,
        AirbagCount = Decimal(row.AirbagCount),
        PowerSteeringIndicator = Decimal(row.PowerSteeringInd),
        AutomaticTransmissionIndicator = Decimal(row.AutomaticTransmissionInd),
        PowerWindowsSource = row.PowerWindowsSource,
        PowerWindowCount = Decimal(row.PowerWindowCount),
        PowerSunroofIndicator = Decimal(row.SunroofInd),
        AlloyWheelsIndicator = Decimal(row.AlloyWheelsInd),
        CargoBoxIndicator = Decimal(row.CargoBoxInd),
        BodyType = Text(texts, TextTable.BodyType, row.BodyType),
        TrimLevel = row.TrimLevel,
        FuelCode = Code(row.FuelCode),
        FuelName = Text(texts, TextTable.Fuel, row.FuelCode),
        DoorCount = Decimal(row.DoorCount),
        Horsepower = Decimal(row.Horsepower),
        SeatCount = Decimal(row.SeatCount),
        StabilityControlIndicator = Decimal(row.StabilityControlInd),
        TowingCapacityWithBrakes = Decimal(row.TowingCapacityBraked),
        TowingCapacityWithoutBrakes = Decimal(row.TowingCapacityUnbraked),
        HomologationTypeCode = Code(row.HomologationTypeCode),
        HomologationTypeName = Text(texts, TextTable.HomologationType, row.HomologationTypeCode),
        ConverterTypeCode = Code(row.ConverterTypeCode),
        ConverterTypeName = Text(texts, TextTable.ConverterType, row.ConverterTypeCode),
        DriveTechnologyCode = Code(row.DriveTechnologyCode),
        DriveTechnologyName = Text(texts, TextTable.DriveTechnology, row.DriveTechnologyCode),
        Co2Amount = Decimal(row.Co2),
        NoxAmount = Decimal(row.Nox),
        Pm10Amount = Decimal(row.Pm10),
        HcAmount = Decimal(row.Hc),
        HcNoxAmount = Decimal(row.HcNox),
        CoAmount = Decimal(row.Co),
        Co2AmountCity = Decimal(row.Co2City),
        NoxAmountCity = Decimal(row.NoxCity),
        Pm10AmountCity = Decimal(row.Pm10City),
        HcAmountCity = Decimal(row.HcCity),
        CoAmountCity = Decimal(row.CoCity),
        Co2AmountHighway = Decimal(row.Co2Highway),
        NoxAmountHighway = Decimal(row.NoxHighway),
        Pm10AmountHighway = Decimal(row.Pm10Highway),
        HcAmountHighway = Decimal(row.HcHighway),
        CoAmountHighway = Decimal(row.CoHighway),
        GreenScore = Decimal(row.GreenIndex),
        PollutionGroup = Decimal(row.PollutionGroup),
        LaneDepartureControlIndicator = Decimal(row.LaneDepartureControlInd),
        LaneDepartureControlRegulationSource = row.LaneDepartureControlSource,
        ForwardDistanceMonitoringIndicator = Decimal(row.ForwardDistanceMonitoringInd),
        ForwardDistanceMonitoringRegulationSource = row.ForwardDistanceMonitoringSource,
        BlindSpotDetectionIndicator = Decimal(row.BlindSpotDetectionInd),
        AdaptiveCruiseControlIndicator = Decimal(row.AdaptiveCruiseControlInd),
        PedestrianDetectionIndicator = Decimal(row.PedestrianDetectionInd),
        PedestrianDetectionRegulationSource = row.PedestrianDetectionSource,
        BrakeAssistIndicator = Decimal(row.BrakeAssistInd),
        ReverseCameraIndicator = Decimal(row.ReverseCameraInd),
        TirePressureMonitoringIndicator = Decimal(row.TirePressureSensorsInd),
        SeatbeltReminderIndicator = Decimal(row.SeatbeltSensorsInd),
        SafetyScore = Decimal(row.SafetyScore),
        SafetyEquipmentLevel = Decimal(row.SafetyEquipmentLevel),
        AutomaticLightingForwardTravelIndicator = Decimal(row.AutoHeadlightsInd),
        AutomaticHighBeamControlIndicator = Decimal(row.AutoHighBeamInd),
        AutomaticHighBeamControlRegulationSource = row.AutoHighBeamSource,
        DangerousApproachDetectionIndicator = Decimal(row.DangerousApproachDetectionInd),
        TrafficSignRecognitionIndicator = Decimal(row.TrafficSignRecognitionInd),
        TwoWheeledVehicleDetection = Decimal(row.TwoWheelerDetection),
        TrafficSignRecognitionRegulationSource = row.TrafficSignRecognitionSource,
        Co2Wltp = Decimal(row.Co2Wltp),
        HcWltp = Decimal(row.HcWltp),
        PmWltp = Decimal(row.PmWltp),
        NoxWltp = Decimal(row.NoxWltp),
        CoWltp = Decimal(row.CoWltp),
        Co2WltpNedc = Decimal(row.Co2WltpNedc),
        ActiveLaneControl = Decimal(row.ActiveLaneKeeping),
        AutomaticBrakingReverseTravel = Decimal(row.ReverseAutoBraking),
        SpeedControlIsa = Decimal(row.IntelligentSpeedAssist),
        EmergencyBrakingPedestriansCyclists = Decimal(row.PedestrianCyclistEmergencyBraking),
        SideCollisionBlindSpot = Decimal(row.BlindSpotSideCollision),
        AlcoLock = Decimal(row.AlcoholInterlock),
        BatteryVoltageDg = Decimal(row.BatteryVoltageClass),
        CommercialName = row.CommercialName
    };

    /// <summary>Source code 0 means the catalog stored a name with no code.</summary>
    private static string? Code(int? id) =>
        id is null or 0 ? null : id.Value.ToString(CultureInfo.InvariantCulture);

    private static string? Decimal(decimal? value) =>
        value?.ToString(CultureInfo.InvariantCulture);

    private static string? Text(
        IReadOnlyDictionary<(int TableId, int Id), string> texts, TextTable table, int? id) =>
        id is int value && texts.TryGetValue(((int)table, value), out var text) ? text : null;
}

/// <summary>Distinct (manufacturer, model) pair projected out of the raw WLTP rows for the list endpoint.</summary>
public class ManufacturerModelSummary
{
    public string? ManufacturerName { get; set; }
    public string? ModelName { get; set; }
    public string? CommercialName { get; set; }
}
