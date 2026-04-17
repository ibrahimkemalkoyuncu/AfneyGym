namespace AfneyGym.Domain.Interfaces;

public enum JoinLessonStatus
{
    Success = 0,
    LessonNotFound = 1,
    NoActiveSubscription = 2,
    CapacityFull = 3,
    AlreadyJoined = 4,
    TimeConflict = 5
}

public enum CancelJoinStatus
{
    Success = 0,
    LessonNotFound = 1,
    NotJoined = 2
}

