namespace CVAgentApp.Core.Enums;

public enum DocumentType
{
    CV = 1,
    CoverLetter = 2,
    Portfolio = 3
}

public enum DocumentStatus
{
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Expired = 4
}

public enum SessionStatus
{
    Created = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Expired = 5
}

public enum EmploymentType
{
    FullTime = 1,
    PartTime = 2,
    Contract = 3,
    Internship = 4,
    Temporary = 5
}

public enum ExperienceLevel
{
    Entry = 1,
    Mid = 2,
    Senior = 3,
    Lead = 4,
    Executive = 5
}

public enum QualificationType
{
    Education = 1,
    Experience = 2,
    Certification = 3,
    Skill = 4,
    Other = 5
}

public enum SkillLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}

public enum SkillCategory
{
    Technical = 1,
    Soft = 2,
    Language = 3,
    Industry = 4
}
