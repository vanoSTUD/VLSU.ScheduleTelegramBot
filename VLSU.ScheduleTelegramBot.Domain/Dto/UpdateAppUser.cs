﻿namespace VLSU.ScheduleTelegramBot.Domain.Dto;

public class UpdateAppUser
{
    public long ChatId { get; set; }
    public bool LooksAtTeachers { get; set; }
}