using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using DataGovIL.Api.Models;
using DataGovIL.Api.Services;

namespace DataGovIL.Api.Data;

/// <summary>
/// One WLTP make/model/year row stored in Postgres, enriched with active/inactive vehicle
/// counts. Unique on (<see cref="ManufacturerCode"/>, <see cref="ModelCode"/>,
/// <see cref="ModelYear"/>, <see cref="ModelType"/>) — the source has duplicate
/// (manufacturer, model, year) triples that differ only by model type.
/// Manufacturer name, brand and country live in <see cref="ManufacturerEntity"/>; join on
/// <see cref="ManufacturerCode"/> (left join — a few WLTP codes are missing from that list).
/// </summary>
[Table("manufacturer_models")]
public class ManufacturerModelEntity : ISyncedEntity
{
    [Column("manufacturer_code")] public int ManufacturerCode { get; set; }
    [Column("model_code")] public int ModelCode { get; set; }
    [Column("model_year")] public int ModelYear { get; set; }
    [Column("model_type")] public string ModelType { get; set; } = string.Empty;

    [Column("model_name")] public string? ModelName { get; set; }
    [Column("commercial_name")] public string? CommercialName { get; set; }
    [Column("trim_level")] public string? TrimLevel { get; set; }
    /// <summary>texts id (table <see cref="TextTable.BodyType"/>), from merkav.</summary>
    [Column("body_type")] public int? BodyType { get; set; }

    [Column("fee_group_code")] public decimal? FeeGroupCode { get; set; }
    [Column("engine_displacement")] public decimal? EngineDisplacement { get; set; }
    [Column("total_weight")] public decimal? TotalWeight { get; set; }
    [Column("height")] public decimal? Height { get; set; }
    /// <summary>texts id (table <see cref="TextTable.Drive"/>).</summary>
    [Column("drive_code")] public int? DriveCode { get; set; }

    /// <summary>texts id (table <see cref="TextTable.DriveTechnology"/>).</summary>
    [Column("drive_technology_code")] public int? DriveTechnologyCode { get; set; }

    /// <summary>texts id (table <see cref="TextTable.Fuel"/>).</summary>
    [Column("fuel_code")] public int? FuelCode { get; set; }
    [Column("horsepower")] public decimal? Horsepower { get; set; }
    [Column("door_count")] public decimal? DoorCount { get; set; }
    [Column("seat_count")] public decimal? SeatCount { get; set; }
    [Column("towing_capacity_braked")] public decimal? TowingCapacityBraked { get; set; }
    [Column("towing_capacity_unbraked")] public decimal? TowingCapacityUnbraked { get; set; }
    /// <summary>texts id (table <see cref="TextTable.HomologationType"/>).</summary>
    [Column("homologation_type_code")] public int? HomologationTypeCode { get; set; }

    /// <summary>texts id (table <see cref="TextTable.ConverterType"/>).</summary>
    [Column("converter_type_code")] public int? ConverterTypeCode { get; set; }

    [Column("air_conditioning_ind")] public decimal? AirConditioningInd { get; set; }
    [Column("abs_ind")] public decimal? AbsInd { get; set; }
    [Column("airbags_source")] public string? AirbagsSource { get; set; }
    [Column("airbag_count")] public decimal? AirbagCount { get; set; }
    [Column("power_steering_ind")] public decimal? PowerSteeringInd { get; set; }
    [Column("automatic_transmission_ind")] public decimal? AutomaticTransmissionInd { get; set; }
    [Column("power_windows_source")] public string? PowerWindowsSource { get; set; }
    [Column("power_window_count")] public decimal? PowerWindowCount { get; set; }
    [Column("sunroof_ind")] public decimal? SunroofInd { get; set; }
    [Column("alloy_wheels_ind")] public decimal? AlloyWheelsInd { get; set; }
    [Column("cargo_box_ind")] public decimal? CargoBoxInd { get; set; }
    [Column("stability_control_ind")] public decimal? StabilityControlInd { get; set; }

    [Column("co2")] public decimal? Co2 { get; set; }
    [Column("nox")] public decimal? Nox { get; set; }
    [Column("pm10")] public decimal? Pm10 { get; set; }
    [Column("hc")] public decimal? Hc { get; set; }
    [Column("hc_nox")] public decimal? HcNox { get; set; }
    [Column("co")] public decimal? Co { get; set; }
    [Column("co2_city")] public decimal? Co2City { get; set; }
    [Column("nox_city")] public decimal? NoxCity { get; set; }
    [Column("pm10_city")] public decimal? Pm10City { get; set; }
    [Column("hc_city")] public decimal? HcCity { get; set; }
    [Column("co_city")] public decimal? CoCity { get; set; }
    [Column("co2_highway")] public decimal? Co2Highway { get; set; }
    [Column("nox_highway")] public decimal? NoxHighway { get; set; }
    [Column("pm10_highway")] public decimal? Pm10Highway { get; set; }
    [Column("hc_highway")] public decimal? HcHighway { get; set; }
    [Column("co_highway")] public decimal? CoHighway { get; set; }
    [Column("co2_wltp")] public decimal? Co2Wltp { get; set; }
    [Column("hc_wltp")] public decimal? HcWltp { get; set; }
    [Column("pm_wltp")] public decimal? PmWltp { get; set; }
    [Column("nox_wltp")] public decimal? NoxWltp { get; set; }
    [Column("co_wltp")] public decimal? CoWltp { get; set; }
    [Column("co2_wltp_nedc")] public decimal? Co2WltpNedc { get; set; }
    [Column("green_index")] public decimal? GreenIndex { get; set; }
    [Column("pollution_group")] public decimal? PollutionGroup { get; set; }

    [Column("safety_score")] public decimal? SafetyScore { get; set; }
    [Column("safety_equipment_level")] public decimal? SafetyEquipmentLevel { get; set; }
    [Column("lane_departure_control_ind")] public decimal? LaneDepartureControlInd { get; set; }
    [Column("lane_departure_control_source")] public string? LaneDepartureControlSource { get; set; }
    [Column("forward_distance_monitoring_ind")] public decimal? ForwardDistanceMonitoringInd { get; set; }
    [Column("forward_distance_monitoring_source")] public string? ForwardDistanceMonitoringSource { get; set; }
    [Column("blind_spot_detection_ind")] public decimal? BlindSpotDetectionInd { get; set; }
    [Column("adaptive_cruise_control_ind")] public decimal? AdaptiveCruiseControlInd { get; set; }
    [Column("pedestrian_detection_ind")] public decimal? PedestrianDetectionInd { get; set; }
    [Column("pedestrian_detection_source")] public string? PedestrianDetectionSource { get; set; }
    [Column("brake_assist_ind")] public decimal? BrakeAssistInd { get; set; }
    [Column("reverse_camera_ind")] public decimal? ReverseCameraInd { get; set; }
    [Column("tire_pressure_sensors_ind")] public decimal? TirePressureSensorsInd { get; set; }
    [Column("seatbelt_sensors_ind")] public decimal? SeatbeltSensorsInd { get; set; }
    [Column("auto_headlights_ind")] public decimal? AutoHeadlightsInd { get; set; }
    [Column("auto_high_beam_ind")] public decimal? AutoHighBeamInd { get; set; }
    [Column("auto_high_beam_source")] public string? AutoHighBeamSource { get; set; }
    [Column("dangerous_approach_detection_ind")] public decimal? DangerousApproachDetectionInd { get; set; }
    [Column("traffic_sign_recognition_ind")] public decimal? TrafficSignRecognitionInd { get; set; }
    [Column("traffic_sign_recognition_source")] public string? TrafficSignRecognitionSource { get; set; }
    [Column("two_wheeler_detection")] public decimal? TwoWheelerDetection { get; set; }
    [Column("active_lane_keeping")] public decimal? ActiveLaneKeeping { get; set; }
    [Column("reverse_auto_braking")] public decimal? ReverseAutoBraking { get; set; }
    [Column("intelligent_speed_assist")] public decimal? IntelligentSpeedAssist { get; set; }
    [Column("pedestrian_cyclist_emergency_braking")] public decimal? PedestrianCyclistEmergencyBraking { get; set; }
    [Column("blind_spot_side_collision")] public decimal? BlindSpotSideCollision { get; set; }
    [Column("alcohol_interlock")] public decimal? AlcoholInterlock { get; set; }
    [Column("battery_voltage_class")] public decimal? BatteryVoltageClass { get; set; }

    /// <summary>From the vehicle-counts resource (<c>mispar_rechavim_pailim</c>).</summary>
    [Column("active_vehicle_count")] public decimal? ActiveVehicleCount { get; set; }

    /// <summary>From the vehicle-counts resource (<c>mispar_rechavim_le_pailim</c>).</summary>
    [Column("inactive_vehicle_count")] public decimal? InactiveVehicleCount { get; set; }

    [Column("content_hash")] public string? ContentHash { get; set; }
    [Column("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Parses the CKAN key columns. Returns false when any of them is missing or not an integer.
    /// </summary>
    public static bool TryParseKey(
        string? manufacturerCode,
        string? modelCode,
        string? modelYear,
        string? modelType,
        out ModelKey key)
    {
        key = default;
        if (ToDecimal(manufacturerCode) is not { } mfr || mfr != decimal.Truncate(mfr)
            || ToDecimal(modelCode) is not { } model || model != decimal.Truncate(model)
            || ToDecimal(modelYear) is not { } year || year != decimal.Truncate(year))
        {
            return false;
        }

        key = new ModelKey((int)mfr, (int)model, (int)year, modelType ?? string.Empty);
        return true;
    }

    public static ManufacturerModelEntity FromDatastore(
        ModelKey key,
        ManufacturerModelDatastoreRecord r,
        VehicleModelCountDatastoreRecord? counts,
        TextCatalog texts) => new()
    {
        ManufacturerCode = key.ManufacturerCode,
        ModelCode = key.ModelCode,
        ModelYear = key.ModelYear,
        ModelType = key.ModelType,

        ModelName = r.ModelName,
        CommercialName = r.CommercialName,
        TrimLevel = r.TrimLevel,
        BodyType = texts.ResolveGenerated(TextTable.BodyType, r.BodyType),

        FeeGroupCode = ToDecimal(r.FeeGroupCode),
        EngineDisplacement = ToDecimal(r.EngineDisplacement),
        TotalWeight = ToDecimal(r.TotalWeight),
        Height = ToDecimal(r.Height),
        DriveCode = texts.ResolveCode(TextTable.Drive, r.DriveCode, r.DriveName),
        DriveTechnologyCode = texts.ResolveCode(TextTable.DriveTechnology, r.DriveTechnologyCode, r.DriveTechnologyName),
        FuelCode = texts.ResolveCode(TextTable.Fuel, r.FuelCode, r.FuelName),
        Horsepower = ToDecimal(r.Horsepower),
        DoorCount = ToDecimal(r.DoorCount),
        SeatCount = ToDecimal(r.SeatCount),
        TowingCapacityBraked = ToDecimal(r.TowingCapacityWithBrakes),
        TowingCapacityUnbraked = ToDecimal(r.TowingCapacityWithoutBrakes),
        HomologationTypeCode = texts.ResolveCode(TextTable.HomologationType, r.HomologationTypeCode, r.HomologationTypeName),
        ConverterTypeCode = texts.ResolveCode(TextTable.ConverterType, r.ConverterTypeCode, r.ConverterTypeName),

        AirConditioningInd = ToDecimal(r.AirConditioningIndicator),
        AbsInd = ToDecimal(r.AbsIndicator),
        AirbagsSource = r.AirbagsSource,
        AirbagCount = ToDecimal(r.AirbagCount),
        PowerSteeringInd = ToDecimal(r.PowerSteeringIndicator),
        AutomaticTransmissionInd = ToDecimal(r.AutomaticTransmissionIndicator),
        PowerWindowsSource = r.PowerWindowsSource,
        PowerWindowCount = ToDecimal(r.PowerWindowCount),
        SunroofInd = ToDecimal(r.PowerSunroofIndicator),
        AlloyWheelsInd = ToDecimal(r.AlloyWheelsIndicator),
        CargoBoxInd = ToDecimal(r.CargoBoxIndicator),
        StabilityControlInd = ToDecimal(r.StabilityControlIndicator),

        Co2 = ToDecimal(r.Co2Amount),
        Nox = ToDecimal(r.NoxAmount),
        Pm10 = ToDecimal(r.Pm10Amount),
        Hc = ToDecimal(r.HcAmount),
        HcNox = ToDecimal(r.HcNoxAmount),
        Co = ToDecimal(r.CoAmount),
        Co2City = ToDecimal(r.Co2AmountCity),
        NoxCity = ToDecimal(r.NoxAmountCity),
        Pm10City = ToDecimal(r.Pm10AmountCity),
        HcCity = ToDecimal(r.HcAmountCity),
        CoCity = ToDecimal(r.CoAmountCity),
        Co2Highway = ToDecimal(r.Co2AmountHighway),
        NoxHighway = ToDecimal(r.NoxAmountHighway),
        Pm10Highway = ToDecimal(r.Pm10AmountHighway),
        HcHighway = ToDecimal(r.HcAmountHighway),
        CoHighway = ToDecimal(r.CoAmountHighway),
        Co2Wltp = ToDecimal(r.Co2Wltp),
        HcWltp = ToDecimal(r.HcWltp),
        PmWltp = ToDecimal(r.PmWltp),
        NoxWltp = ToDecimal(r.NoxWltp),
        CoWltp = ToDecimal(r.CoWltp),
        Co2WltpNedc = ToDecimal(r.Co2WltpNedc),
        GreenIndex = ToDecimal(r.GreenScore),
        PollutionGroup = ToDecimal(r.PollutionGroup),

        SafetyScore = ToDecimal(r.SafetyScore),
        SafetyEquipmentLevel = ToDecimal(r.SafetyEquipmentLevel),
        LaneDepartureControlInd = ToDecimal(r.LaneDepartureControlIndicator),
        LaneDepartureControlSource = r.LaneDepartureControlRegulationSource,
        ForwardDistanceMonitoringInd = ToDecimal(r.ForwardDistanceMonitoringIndicator),
        ForwardDistanceMonitoringSource = r.ForwardDistanceMonitoringRegulationSource,
        BlindSpotDetectionInd = ToDecimal(r.BlindSpotDetectionIndicator),
        AdaptiveCruiseControlInd = ToDecimal(r.AdaptiveCruiseControlIndicator),
        PedestrianDetectionInd = ToDecimal(r.PedestrianDetectionIndicator),
        PedestrianDetectionSource = r.PedestrianDetectionRegulationSource,
        BrakeAssistInd = ToDecimal(r.BrakeAssistIndicator),
        ReverseCameraInd = ToDecimal(r.ReverseCameraIndicator),
        TirePressureSensorsInd = ToDecimal(r.TirePressureMonitoringIndicator),
        SeatbeltSensorsInd = ToDecimal(r.SeatbeltReminderIndicator),
        AutoHeadlightsInd = ToDecimal(r.AutomaticLightingForwardTravelIndicator),
        AutoHighBeamInd = ToDecimal(r.AutomaticHighBeamControlIndicator),
        AutoHighBeamSource = r.AutomaticHighBeamControlRegulationSource,
        DangerousApproachDetectionInd = ToDecimal(r.DangerousApproachDetectionIndicator),
        TrafficSignRecognitionInd = ToDecimal(r.TrafficSignRecognitionIndicator),
        TrafficSignRecognitionSource = r.TrafficSignRecognitionRegulationSource,
        TwoWheelerDetection = ToDecimal(r.TwoWheeledVehicleDetection),
        ActiveLaneKeeping = ToDecimal(r.ActiveLaneControl),
        ReverseAutoBraking = ToDecimal(r.AutomaticBrakingReverseTravel),
        IntelligentSpeedAssist = ToDecimal(r.SpeedControlIsa),
        PedestrianCyclistEmergencyBraking = ToDecimal(r.EmergencyBrakingPedestriansCyclists),
        BlindSpotSideCollision = ToDecimal(r.SideCollisionBlindSpot),
        AlcoholInterlock = ToDecimal(r.AlcoLock),
        BatteryVoltageClass = ToDecimal(r.BatteryVoltageDg),

        ActiveVehicleCount = ToDecimal(counts?.ActiveVehicleCount),
        InactiveVehicleCount = ToDecimal(counts?.InactiveVehicleCount)
    };

    private static decimal? ToDecimal(string? raw) =>
        decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : null;
}

/// <summary>Primary key of <see cref="ManufacturerModelEntity"/>.</summary>
public readonly record struct ModelKey(int ManufacturerCode, int ModelCode, int ModelYear, string ModelType);
