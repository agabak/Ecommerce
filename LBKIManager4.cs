using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Common.BLL.VSAM;
using Common.BLL.ParamsInterface;
using Crim.BLL.GeneralInquiryPageManger.Interface;

namespace GeneralInquiryPageManger
{
    [Serializable]
    public class LBKIManager : VSamBusinessBase<LBKIManager>
    {
        private readonly List<ILBKIRecordStrategy> strategies;

        public List<LBKI_DTO> LBKIOutput { get; private set; }
        public List<LBKI_DTO_AA> LBKIOutputAA { get; private set; }
        public List<LBKI_DTO_PMDATA> LBKIOutputPMDATA { get; private set; }

        public LBKIManager()
        {
            strategies = new List<ILBKIRecordStrategy>
            {
                new AARecordStrategy(),
                new ACRecordStrategy(),
                new ADRecordStrategy(),
                new AIRecordStrategy(),
                new AURecordStrategy(),
                new YJRecordStrategy(),
                new YKRecordStrategy(),
                new JZRecordStrategy()
            };
        }

        [Serializable]
        private class Criteria
        {
            public string SPN;
            public bool Paging;
            public int CurrentPage;
            public int PageSize;
            public string SortField;
            public string SortDirection;
        }

        public static LBKIManager Get(
            string spn,
            bool paging,
            int currentPage,
            int pageSize,
            string sortField,
            string sortDirection)
        {
            var crit = new Criteria
            {
                SPN = spn,
                Paging = paging,
                CurrentPage = currentPage,
                PageSize = pageSize,
                SortField = sortField,
                SortDirection = sortDirection
            };

            return DataPortal.Fetch<LBKIManager>(crit);
        }

        private void DataPortal_Fetch(Criteria crit)
        {
            var context = new LBKIContext
            {
                SPN = crit.SPN
            };

            var result = new LBKIResult();

            foreach (var strategy in strategies)
            {
                strategy.Execute(context, result);
            }

            LBKIOutput = result.Cases;
            LBKIOutputAA = result.Names;
            LBKIOutputPMDATA = new List<LBKI_DTO_PMDATA> { result.PersonData };
        }
    }


    public interface ILBKIRecordStrategy
    {
        void Execute(LBKIContext context, LBKIResult result);
    }


    public class LBKIContext
    {
        public string SPN;

        public MainIDNumbers AIs;
        public FugitiveApprehensionUnits AUs;
        public SPNBookings YJs;
        public HoldForBookings YKs;
        public RelatedCases JZs;

        public OffenseClassCodes OffCodes;
    }


    public class LBKIResult
    {
        public List<LBKI_DTO> Cases = new();
        public List<LBKI_DTO_AA> Names = new();
        public LBKI_DTO_PMDATA PersonData = new();
    }


    public class AARecordStrategy : ILBKIRecordStrategy
    {
        public void Execute(LBKIContext ctx, LBKIResult result)
        {
            var names = PersonMasterNames
                .Get(ctx.SPN, "AA")
                .Sort("ClassSequenceNumber", false);

            foreach (var n in names)
            {
                if (n.ClassSequenceNumber != "999" &&
                    n.ClassSequenceNumber != "998")
                    continue;

                result.Names.Add(new LBKI_DTO_AA
                {
                    Name = n.Name,
                    PTY = n.PersonType,
                    RAC = n.Race,
                    SEX = n.Sex,
                    DOB = DateValidators.ConvertDateString(n.DateOfBirth, "MM/dd/yy"),
                    USC = n.UsCitizens,
                    JAIL = n.JailInd,
                    WW = n.WarrantInd,
                    CIN = n.CautionInd,
                    SPN = n.SystemPersonNum,
                    CLS = n.ClassSequenceNumber
                });

                if (!string.IsNullOrWhiteSpace(n.Height))
                    result.PersonData.HGT = n.Height;

                if (!string.IsNullOrWhiteSpace(n.Weight))
                    result.PersonData.WGT = n.Weight;

                if (!string.IsNullOrWhiteSpace(n.ColorOfEyes))
                    result.PersonData.EYES = n.ColorOfEyes;

                if (!string.IsNullOrWhiteSpace(n.ColorOfHair))
                    result.PersonData.HAIR = n.ColorOfHair;

                if (!string.IsNullOrWhiteSpace(n.SkinTone))
                    result.PersonData.SKIN = n.SkinTone;

                if (!string.IsNullOrWhiteSpace(n.Build))
                    result.PersonData.BLD = n.Build;
            }
        }
    }


    public class ACRecordStrategy : ILBKIRecordStrategy
    {
        public void Execute(LBKIContext ctx, LBKIResult result)
        {
            var comments = IDComments.Get(ctx.SPN, "AC", "", "", "", "");

            if (comments.Count == 0)
                return;

            var c = comments.First();

            if (c.SubclassSeqNum == "00" && c.ContinuationCount == "0")
            {
                result.PersonData.CAUTION = "** SEE LP13 FOR CAUTIONS **";
            }
        }
    }


    public class ADRecordStrategy : ILBKIRecordStrategy
    {
        public void Execute(LBKIContext ctx, LBKIResult result)
        {
            var ads = AddressPhoneOccupations.Get(ctx.SPN, "AD");

            if (ads.Count == 0)
                return;

            var ad = ads.FirstOrDefault();

            if (ad == null)
                return;

            result.PersonData.STN = ad.StreetNumber;
            result.PersonData.STD = ad.StreetDirection;
            result.PersonData.SNA = ad.StreetName;
            result.PersonData.APN = ad.ApartmentNumberPmadapn;
            result.PersonData.ADC = ad.AddressCity;
            result.PersonData.ADS = ad.State;
            result.PersonData.AZC = ad.zipeCode;
            result.PersonData.PHN = ad.TelephoneNumber;
        }
    }


    public class AIRecordStrategy : ILBKIRecordStrategy
    {
        public void Execute(LBKIContext ctx, LBKIResult result)
        {
            ctx.AIs ??= MainIDNumbers
                .Get(ctx.SPN, "AI", "", "", "", "0");

            foreach (var ai in ctx.AIs)
            {
                if (ai.SubclassSeqNum != "00")
                    continue;

                if (!string.IsNullOrWhiteSpace(ai.SheriffOfficeNumber))
                    result.PersonData.SON = ai.SheriffOfficeNumber;

                if (!string.IsNullOrWhiteSpace(ai.ScarsMarkAndTattoos))
                    result.PersonData.SMT = ai.ScarsMarkAndTattoos;
            }
        }
    }


    public class AURecordStrategy : ILBKIRecordStrategy
    {
        public void Execute(LBKIContext ctx, LBKIResult result)
        {
            ctx.AUs ??= FugitiveApprehensionUnits.Get(ctx.SPN, "AU", "", "", "", "0");

            foreach (var au in ctx.AUs)
            {
                if (au.SubclassSeqNum != "00")
                    continue;

                result.PersonData.FAU = au.PropertyRecordIndicator;
            }
        }
    }


    public class YJRecordStrategy : ILBKIRecordStrategy
    {
        public void Execute(LBKIContext ctx, LBKIResult result)
        {
            ctx.YJs ??= SPNBooking
                .Get(ctx.SPN, "YJ", "", "", "", "0");

            var booking = ctx.YJs.FirstOrDefault();

            if (booking == null)
                return;

            result.PersonData.PFG = booking.PrisonerFlag;
            result.PersonData.PCL = booking.PrisonerClassification;
            result.PersonData.BOOKINGNO = booking.BookingNumber;

            if (string.IsNullOrWhiteSpace(booking.ReleaseDate))
            {
                result.PersonData.JAILLOC = booking.User;
                result.PersonData.CELLBLKCELLBNK =
                    booking.JailTank +
                    booking.JailCell +
                    booking.JailBunk;
            }
        }
    }


    public class YKRecordStrategy : ILBKIRecordStrategy
    {
        public void Execute(LBKIContext ctx, LBKIResult result)
        {
            ctx.YKs ??= HoldForBookings.Get(ctx.SPN, "YK", "", "", "", "1");

            if (ctx.YKs.Count == 0)
                result.PersonData.HOLDFORS = "N";
            else
                result.PersonData.HOLDFORS = "Y";
        }
    }


    public class JZRecordStrategy : ILBKIRecordStrategy
    {
        public void Execute(LBKIContext ctx, LBKIResult result)
        {
            ctx.JZs ??= RelatedCases.Get(ctx.SPN);

            foreach (var jz in ctx.JZs)
            {
                if (jz.ConnectionCode == "PAD")
                    continue;

                result.Cases.Add(new LBKI_DTO
                {
                    CDI = jz.CourtDivisionInd,
                    CASE = jz.CaseNum,
                    CRT = jz.Court,
                    COC = jz.ConnectionCode,
                    BOND = jz.BondAmount?.ToString().PadLeft(8, '0'),
                    BONDEXCEPTION = jz.BondException
                });
            }
        }
    }


    [Serializable]
    public class LBKI_DTO_AA
    {
        public string Name { get; set; }
        public string USC { get; set; }
        public string PTY { get; set; }
        public string RAC { get; set; }
        public string SEX { get; set; }
        public string DOB { get; set; }
        public string JAIL { get; set; }
        public string WW { get; set; }
        public string CIN { get; set; }
        public string SPN { get; set; }
        public string CLS { get; set; }
    }


    public class LBKI_DTO_PMDATA
    {
        public string HGT { get; set; }
        public string WGT { get; set; }
        public string EYES { get; set; }
        public string HAIR { get; set; }
        public string SKIN { get; set; }
        public string BLD { get; set; }
        public string SMT { get; set; }
        public string RCVLOC { get; set; }
        public string JAILLOC { get; set; }
        public string CELLBLKCELLBNK { get; set; }
        public string BOOKINGNO { get; set; }
        public string STN { get; set; }
        public string PHN { get; set; }
        public string CAUTION { get; set; }
        public string HOLDFORS { get; set; }
        public string SON { get; set; }
        public string FAU { get; set; }
        public string STD { get; set; }
        public string SNA { get; set; }
        public string APN { get; set; }
        public string ADC { get; set; }
        public string ADS { get; set; }
        public string AZC { get; set; }
        public string PFG { get; set; }
        public string PCL { get; set; }
    }


    public class LBKI_DTO
    {
        public string SEQ { get; set; }
        public string CDI { get; set; }
        public string CASE { get; set; }
        public string CRT { get; set; }
        public string BOND { get; set; }
        public string BONDEXCEPTION { get; set; }
        public string BOOKED { get; set; }
        public string RSLED { get; set; }
        public string HOW { get; set; }
        public string OFFENSE { get; set; }
        public string COC { get; set; }
        public string CAUTIONS { get; set; }
    }
}