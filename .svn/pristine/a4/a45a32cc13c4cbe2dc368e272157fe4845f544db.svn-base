using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenerateTables.Models;

namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void PREADetails()
        {
            Db.PREANotes.AddRange(
                new PREANotes
                {
                    PREANotesId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    InmateId = 100,
                    InvestigationFlagIndex = 3,
                    PREANote = "PROGRESS",
                    DeleteFlag = false,
                    CreateBy = 11
                },
                new PREANotes
                {
                    PREANotesId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    InmateId = 101,
                    InvestigationFlagIndex = 2,
                    PREANote = null,
                    DeleteFlag = false,
                    CreateBy = 12
                },
                new PREANotes
                {
                    PREANotesId = 12,
                    CreateDate = DateTime.Now.AddDays(-10),
                    InmateId = 108,
                    InvestigationFlagIndex = 5,
                    PREANote = null,
                    DeleteFlag = false,
                    CreateBy = 11
                }
                );


            Db.PREAReview.AddRange(
                new PREAReview
                {
                    PREAReviewId = 10,
                    CreateDate = DateTime.Now.AddDays(-15),
                    InmateId = 100,
                    DeleteFlag = false,
                    PREAReviewFlagId = 5
                },
                new PREAReview
                {
                    PREAReviewId = 11,
                    CreateDate = DateTime.Now.AddDays(-15),
                    InmateId = 100,
                    DeleteFlag = false,
                    PREAReviewFlagId = 5
                },
                new PREAReview
                {
                    PREAReviewId = 12,
                    CreateDate = DateTime.Now.AddDays(-15),
                    InmateId = 110,
                    DeleteFlag = false,
                    PREAReviewFlagId = 3
                }
               );
        }
    }
}
