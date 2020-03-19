DROP TRIGGER CourtArraignmentDate

GO

CREATE TRIGGER [dbo].[CourtArraignmentDate]
   ON  [dbo].[Schedule]
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
    
    SET NOCOUNT ON;

 

     IF (SELECT top 1 Discriminator FROM INSERTED) = 'ScheduleCourt' 
     BEGIN

 

          -- Insert statements for trigger here
          UPDATE [dbo].[Arrest] 
          SET [arrest_arraignment_date] =  s.[StartDate]
          FROM INSERTED s --[dbo].[Schedule] S
                inner join [dbo].[Arrest]a on s.inmateid = a.Inmate_ID 
          WHERE s.IsSingleOccurrence = 1 and s.DeleteFlag != 1 
                and 
                a.[arrest_arraignment_date] > s.[StartDate]
                or a.[arrest_arraignment_date] IS NULL

 

    END
END

 

GO