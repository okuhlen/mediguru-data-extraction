using System.ComponentModel;

namespace MediGuru.DataExtractionTool.Enumerations;

//TODO: Create MSBuild task that can create an autogenerated enum??
//TODO: This should be renamed to EsDiscipline
public enum EsDoctorType
{
    [Description("Paediatric Surgeon")]
    PaediatricSurgeon,
    
    [Description("Biokinetics")]
    Biokinetics,
    
    [Description("Medical Technology")]
    MedicalTechnology,
    
    [Description("Opthalmology")]
    Opthalmology,
    
    [Description("Medical Scientist - Clinical Biochemist")]
    MedicalScientistClinicalBiochemist,
    
    [Description("Private Hospitals ('B' - Status)")]
    PrivateHospitalsBStatus,
    
    [Description("Clinical technology - Neurophysiology")]
    ClinicalTechnologyNeurophysiology,
    
    [Description("Registered nurses")]
    RegisteredNurses,
    
    [Description("Registered Counsellors")]
    RegisteredCounsellors,
    
    [Description("Radiography")]
    Radiography,
    
    [Description("Physical Medicine")]
    PhysicalMedicine,
    
    [Description("Orthoptists")]
    Orthoptists,
    
    [Description("Periodontics")]
    Periodontics,
    
    [Description("Clinical services - Employer Primary Care Facilities (NAMIBIA Only)")]
    ClinicalServicesEmployerPrimaryCareFacilitiesNamibiaOnly,
    
    [Description("Surgery/Paediatric surgery")]
    SurgeryPaediatricSurgery,
    
    [Description("Clinical technology - Pulmonology/Nephrology")]
    ClinicalTechnologyPulmonologyNephrology,
    
    [Description("Ambulance Services - Intermediate")]
    AmbulanceServicesIntermediate,
    
    [Description("Private Hospitals ('A' - Status) - -ICU +Theatre")]
    PrivateHospitalsAStatusICUPlusTheatre,

    [Description("Nuclear Medicine")]
    NuclearMedicine,
    
    [Description("Medical technology - Pathology")]
    MedicalTechnologyPathology,
    
    [Description("Clinical services - Oxygen Supplier")]
    ClinicalServicesOxygenSupplier,
    
    [Description("Clinical services - Eye Prothetists Supplier")]
    ClinicalServicesEyeProthetistsSupplier,
    
    [Description("Namibian Practioners Only (Not recognized by (HPCSA) - Reflexologists")]
    NamibianPractitionersOnlyNotRecognizedByHPCSAReflexologists,
    
    [Description("Clinical services - FAMSA (family and marriage counselling)")]
    ClinicalServicesFAMSA,
    
    [Description("Paed. Cardiology")]
    PaedCardiology,
    
    [Description("Group practices/Hospitals")]
    GroupPracticesHospitals,

    [Description("Dermatology")]
    Dermatology,
    
    [Description("Orthopaedics")]
    Orthopaedics,
    
    [Description("Medical Scientist - Medical Biological Scientist")]
    MedicalScientistMedicalBiologicalScientist,
    
    [Description("Ambulance Services - Basic")]
    AmbulanceServicesBasic,
    
    [Description("Registered nurses - Midwife only")]
    RegisteredNursesMidwifeOnly,
    
    [Description("Registered nurses - Primary Care")]
    RegisteredNursesPrimaryCare,
    
    [Description("Masseurs")]
    Masseurs,
    
    [Description("Drug & Alcohol Rehab - (Welfare)")]
    DrugAndAlcoholRehabWelfare,
    
    [Description("Neurology")]
    Neurology,
    
    [Description("Provincial Hospitals - Primary Care")]
    ProvincialHospitalsPrimaryCare,
    
    [Description("Private Hospitals ('A' - Status) - +Theatre Maternity only")]
    PrivateHospitalAStatusTheatreMaternityOnly,
    
    [Description("Private Hospitals ('A' - Status) - -ICU -Theatre")]
    PrivateHospitalsAStatusICUTheatre,
    
    [Description("Medical Oncology")]
    MedicalOncology,
    
    [Description("Dental Technician")]
    DentalTechnician,
    
    [Description("Approved U O T U / Day clinics")]
    ApprovedUOTUDayClinics,
    
    [Description("Acupuncturist")]
    Acupuncturist,
    
    [Description("Phytotherapy")]
    Phytotherapy,
    
    [Description("Naturopathy")]
    Naturopathy,
    
    [Description("Namibian Practioners Only (Not recognized by (HPCSA) - Acupuncturist")]
    NamibianPractitionersOnlyNotRecognizedByHPCSAAcupuncturist,
    
    [Description("Chiropractors")]
    Chiropractors,
    
    [Description("Obstetrics and Gynaecology")]
    ObstetricsAndGynaecology,
    
    [Description("Cardio Thoracic Surgery")]
    CardioThoracicSurgery,
    
    [Description("Blood transfusion services")]
    BloodTransfusionServices,
    
    [Description("Group Practice - Primary Care")]
    GroupPracticePrimaryCare,
    
    [Description("Paediatrics")]
    Paediatrics,
    
    [Description("Orthotists & Prosthetists")]
    OrthotistsAndProsthetists,
    
    [Description("Spec.Phys/Int Med/Diabetes/Rheumatology/Nephrology/Endocrino")]
    SpecPhysIntMedDiabetesRheumatologyNephrologyEndocrino,
    
    [Description("Otorhinolaryngology")]
    Otorhinolaryngology,
    
    [Description("Clinical technology - Cardiology")]
    ClinicalTechnologyCardiology,
    
    [Description("Clinical services - Stomal/appliances Supplier")]
    ClinicalServicesStomalAppliancesSupplier,
    
    [Description("Step Down Facilities")]
    StepDownFacilities,
    
    [Description("Private Hospitals ('A' - Status) - State subsidised")]
    PrivateHospitalsAStatusStateSubsidised,
    
    [Description("Optometrists")]
    Optometrists,
    
    [Description("Nursing Agencies/Home Care Services")]
    NursingAgenciesHomeCareServices,
    
    [Description("Optical dispensers")]
    OpticalDispensers,
    
    [Description("Social workers")]
    SocialWorkers,
    
    [Description("Psychologists")]
    Psychologists,
    
    [Description("Gastroenterology")]
    Gastroenterology,
    
    [Description("Psychometry")]
    Psychometry,
    
    [Description("Private Hospitals ('A' - Status) - +ICU +Theatre Less than 100 beds")]
    PrivateHospiralsAStatusICUTheatreLessThan100Beds,
    
    [Description("Anaesthetists")]
    Anaesthetists,
    
    [Description("Speech Therapy")]
    SpeechTherapy,
    
    [Description("Clinical Haemotology")]
    ClinicalHaemotology,
    
    [Description("Step Down Facilities - Physical Rehab")]
    StepDownFacilitiesPhysicalRehab,
    
    [Description("Orthodontics")]
    Orthodontics,
    
    [Description("Clinical services - Ear Prothetists Supplier")]
    ClinicalServicesEarProthetistsSupplier,
    
    [Description("Ambulance Services - Other")]
    AmbulanceServicesOther,
    
    [Description("Mental Health Institutions")]
    MentalHealthInstitutions,
    
    [Description("Provincial Hospitals")]
    ProvincialHospitals,
    
    [Description("Medical Scientist - Genetic Councillor")]
    MedicalScientistGeneticCouncillor,
    
    [Description("Oral Hygienists")]
    OralHygienists,
    
    [Description("Unattached operating theatres / Day clinics")]
    UnattachedOperatingTheatresDayClinics,
    
    [Description("Dieticians")]
    Dieticians,
    
    [Description("Rheumatology")]
    Rheumatology,
    
    [Description("Clinical technology - Critical Care")]
    ClinicalTechnologyCriticalCare,
    
    [Description("Prostodontics")]
    Prostodontics,
    
    [Description("Plastic and Reconstructive Surgery")]
    PlasticAndReconstructiveSurgery,
    
    [Description("Specialist Physician - Clinical Haematology")]
    SpecialistPhysicianClinicalHaematology,
    
    [Description("Ambulance Services - Advanced")]
    AmbulanceServicesAdvanced,
    
    [Description("General Medical Practice")]
    GeneralMedicalPractice,
    
    [Description("Private Rehab Hospital (Acute)")]
    PrivateRehabHospitalAcute,
    
    [Description("Private Hospitals ('A' - Status)")]
    PrivateHospitalsAStatus,
    
    [Description("General Dental Practice")]
    GeneralDentalPractice,
    
    [Description("Art Therapists")]
    ArtTherapists,
    
    [Description("Pharmacies")]
    Pharmacies,
    
    [Description("Registered nurses - Psychiatric only")]
    RegisteredNursesPsychiatricOnly,
    
    [Description("Group practices")]
    GroupPractices,
    
    [Description("Clinical technology - Reproductive biology")]
    ClinicalTechnologyReproductiveBiology,
    
    [Description("Clinical services - Cardiac Prothetists Supplier")]
    ClinicalServicesCardiacProthetistsSupplier,
    
    [Description("Private Hospitals ('A' - Status) - -Theatre Maternity only")]
    PrivateHospitalAStatusTheatreMinusMaternityOnly,
    
    [Description("Hearing Aid Acoustician")]
    HearingAidAcoustician,
    
    [Description("Clinical Technology (General)")]
    ClinicalTechnologyGeneral,
    
    [Description("Radiotherapy/Nuclear Medicine/Oncologist")]
    RadiotherapyNuclearMedicineOncologist,
    
    [Description("Ophthalmology")]
    Ophthalmology,
    
    [Description("Namibian Practioners Only (Not recognized by (HPCSA)")]
    NamibianPractitionersOnlyNotRecognizedByHPCSA,

    [Description("Urology")]
    Urology,
    
    [Description("Speech therapy / Audiology")]
    SpeechTherapyAudiology,
    
    [Description("Step Down Facilities - Chronic")]
    StepDownFacilitiesChronic,
    
    [Description("Audiology")]
    Audiology,
    
    [Description("Clinical services")]
    ClinicalServices,
    
    [Description("Dental therapy")]
    DentalTherapy,
    
    [Description("Homeopaths")]
    Homeopaths,
    
    [Description("Family Physician")]
    FamilyPhysician,
    
    [Description("Pharmacotherapist")]
    Pharmacotherapist,
    
    [Description("Medical Scientist - Medical Physicist")]
    MedicalScientistMedicalPhysicist,
    
    [Description("Private Hospitals ('A' - Status) - +ICU -Theatre")]
    PrivateHospitalAStatusICUTheatre,
    
    [Description("Neurosurgery")]
    Neurosurgery,
    
    [Description("Podiatry")]
    Podiatry,
    
    [Description("Psychiatry")]
    Psychiatry,
    
    [Description("Step Down Facilities - Acute")]
    StepDownFacilitiesAcute,
    
    [Description("Drug & Alcohol Rehab - (Department of Health)")]
    DrugAndAlcoholRehabDepartmentOfHealth,
    
    [Description("Pulmonology")]
    Pulmonology,
    
    [Description("Diagnostic Radiology")]
    DiagnosticRadiology,
    
    [Description("Community health")]
    CommunityHealth,
    
    [Description("Pathology")]
    Pathology,
    
    [Description("Private Hospitals ('A' - Status) - Mine Hospilals")]
    PrivateHospitalAStatusMineHospitals,
    
    [Description("Clinical technology - Cardio-Vascular")]
    ClinicalTechnologyCardioVascular,
    
    [Description("Community dentistry")]
    CommunityDentistry,
    
    [Description("Clinical services - Breast Prothetists  Supplier")]
    ClinicalServicesBreastProthetistsSupplier,

    [Description("Hospices")]
    Hospices,
    
    [Description("Physiotherapists")]
    Physiotherapists,
    
    [Description("Delayed Children Development Clinic - WELFARE & DOH")]
    DelayedChildrenDevelopmentClinicWELFAREDO,

    [Description("Cardiology")]
    Cardiology,
    
    [Description("Maxillo-facial and Oral Surgery")]
    MaxilloFacialAndOralSurgery,
    
    [Description("Hospices - SA Cancer Associations")]
    HospicesSACancerAssociations,
    
    [Description("Clinical services - Medical General Supplier")]
    ClinicalServicesMedicalGeneralSupplier,
    
    [Description("Occupational Therapy")]
    OccupationalTherapy,
    
    [Description("Namibian Practioners Only (Not recognized by (HPCSA) - Forensic")]
    NamibianPractionersOnlyNotRecognizedByHPCSAForensic,
    
    [Description("Oral pathology")]
    OralPathology,
    
    [Description("Clinical services - Wheelchairs Supplier")]
    ClinicalServicesWheelchairsSupplier,

}