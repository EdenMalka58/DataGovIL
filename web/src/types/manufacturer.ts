/**
 * Matches the JSON shape returned by GET /api/manufacturers and
 * GET /api/manufacturers/{manufacturerCode}/{modelCode}.
 * Field names mirror ASP.NET camelCase serialization of ManufacturerModelRecord.
 */
export interface ManufacturerModelRecord {
  id?: string | null;
  modelType?: string | null;
  manufacturerCode?: string | null;
  manufacturerName?: string | null;
  /** Motomarks CDN slug. Present only when that manufacturer has a logo. */
  logoSlug?: string | null;
  manufacturerCountryName?: string | null;
  tozar?: string | null;
  modelCode?: string | null;
  modelName?: string | null;
  modelYear?: string | null;
  feeGroupCode?: string | null;
  /** Estimated monthly usage value (₪), computed by the API from feeGroupCode. */
  feeGroupPrice?: number | null;
  /** Estimated annual license fee (₪), computed by the API from feeGroupCode and the vehicle's first registration date. Null on catalog rows. */
  estimatedLicenseFee?: number | null;
  engineDisplacement?: string | null;
  totalWeight?: string | null;
  curbWeight?: string | null;
  liftingLoadWeight?: string | null;
  height?: string | null;
  driveCode?: string | null;
  driveName?: string | null;
  airConditioningIndicator?: string | null;
  absIndicator?: string | null;
  airbagsSource?: string | null;
  airbagCount?: string | null;
  powerSteeringIndicator?: string | null;
  automaticTransmissionIndicator?: string | null;
  powerWindowsSource?: string | null;
  powerWindowCount?: string | null;
  powerSunroofIndicator?: string | null;
  alloyWheelsIndicator?: string | null;
  cargoBoxIndicator?: string | null;
  bodyType?: string | null;
  trimLevel?: string | null;
  fuelCode?: string | null;
  fuelName?: string | null;
  doorCount?: string | null;
  horsepower?: string | null;
  seatCount?: string | null;
  stabilityControlIndicator?: string | null;
  towingCapacityWithBrakes?: string | null;
  towingCapacityWithoutBrakes?: string | null;
  homologationTypeCode?: string | null;
  homologationTypeName?: string | null;
  euTypeApproval?: string | null;
  converterTypeCode?: string | null;
  converterTypeName?: string | null;
  driveTechnologyCode?: string | null;
  driveTechnologyName?: string | null;
  co2Amount?: string | null;
  noxAmount?: string | null;
  pm10Amount?: string | null;
  hcAmount?: string | null;
  hcNoxAmount?: string | null;
  coAmount?: string | null;
  co2AmountCity?: string | null;
  noxAmountCity?: string | null;
  pm10AmountCity?: string | null;
  hcAmountCity?: string | null;
  coAmountCity?: string | null;
  co2AmountHighway?: string | null;
  noxAmountHighway?: string | null;
  pm10AmountHighway?: string | null;
  hcAmountHighway?: string | null;
  coAmountHighway?: string | null;
  greenScore?: string | null;
  pollutionGroup?: string | null;
  laneDepartureControlIndicator?: string | null;
  laneDepartureControlRegulationSource?: string | null;
  forwardDistanceMonitoringIndicator?: string | null;
  forwardDistanceMonitoringRegulationSource?: string | null;
  blindSpotDetectionIndicator?: string | null;
  adaptiveCruiseControlIndicator?: string | null;
  pedestrianDetectionIndicator?: string | null;
  pedestrianDetectionRegulationSource?: string | null;
  brakeAssistIndicator?: string | null;
  reverseCameraIndicator?: string | null;
  tirePressureMonitoringIndicator?: string | null;
  seatbeltReminderIndicator?: string | null;
  safetyScore?: string | null;
  safetyEquipmentLevel?: string | null;
  automaticLightingForwardTravelIndicator?: string | null;
  automaticHighBeamControlIndicator?: string | null;
  automaticHighBeamControlRegulationSource?: string | null;
  dangerousApproachDetectionIndicator?: string | null;
  trafficSignRecognitionIndicator?: string | null;
  twoWheeledVehicleDetection?: string | null;
  trafficSignRecognitionRegulationSource?: string | null;
  co2Wltp?: string | null;
  hcWltp?: string | null;
  pmWltp?: string | null;
  noxWltp?: string | null;
  coWltp?: string | null;
  co2WltpNedc?: string | null;
  activeLaneControl?: string | null;
  automaticBrakingReverseTravel?: string | null;
  speedControlIsa?: string | null;
  emergencyBrakingPedestriansCyclists?: string | null;
  sideCollisionBlindSpot?: string | null;
  alcoLock?: string | null;
  batteryVoltageDg?: string | null;
  commercialName?: string | null;
  extensionData?: Record<string, unknown> | null;
}

export interface ManufacturerModelSummary {
  manufacturerName?: string | null;
  modelName?: string | null;
  commercialName?: string | null;
}
