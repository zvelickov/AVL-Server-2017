using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.HttpListener
{
        public class JsonCompany
        {


            private long mIdCompany;

            public long IdCompany
            {
                get { return mIdCompany; }
                set { mIdCompany = value; }
            }

            private string mCompanyname;

            public string CompanyName
            {
                get { return mCompanyname; }
                set { mCompanyname = value; }
            }

        }


        public class JsonResponse
        {

            private long mResponseCode;

            public long ResponseCode
            {
                get { return mResponseCode; }
                set { mResponseCode = value; }
            }

            private string mResponseDescription;

            public string ResponseDescription
            {
                get { return mResponseDescription; }
                set { mResponseDescription = value; }
            }          
        }


        public class JsonReasonsForUserCancelation
        {

            private long mReasonsForUserCancelation;

            public long IdReasonsForUserCancelation
            {
                get { return mReasonsForUserCancelation; }
                set { mReasonsForUserCancelation = value; }
            }

            private string mReasonsForUserCancelationText;

            public string ReasonsForUserCancelationText
            {
                get { return mReasonsForUserCancelationText; }
                set { mReasonsForUserCancelationText = value; }
            }          
        }



        public class JsonPaymentOptions
        {
            private long mPaymentOption;

            public long IdPaymentOption
            {
                get { return mPaymentOption; }
                set { mPaymentOption = value; }
            }

            private string mPaymentOptionText;

            public string PaymentOptionText
            {
                get { return mPaymentOptionText; }
                set { mPaymentOptionText = value; }
            }
        }


        public class JsonVehicle
        {


            private long mIdVehicle;

            public long IdVehicle
            {
                get { return mIdVehicle; }
                set { mIdVehicle = value; }
            }

            private string mVehiclePlate;

            public string VehiclePlate
            {
                get { return mVehiclePlate; }
                set { mVehiclePlate = value; }
            }


            private string mVehicleShortDescription;

            public string VehicleShortDescription
            {
                get { return mVehicleShortDescription; }
                set { mVehicleShortDescription = value; }
            }

            private string mVehicleState;

            public string VehicleState
            {
                get { return mVehicleState; }
                set { mVehicleState = value; }
            }
        }


        public class JsonVehicleLocation
        {


            private long mIdVehicle;

            public long IdVehicle
            {
                get { return mIdVehicle; }
                set { mIdVehicle = value; }
            }

            private double mLongitudeX;

            public double LongitudeX
            {
                get { return mLongitudeX; }
                set { mLongitudeX = value; }
            }


            private double mLatitudeY;

            public double LatitudeY
            {
                get { return mLatitudeY; }
                set { mLatitudeY = value; }
            }


            private double mBearing;

            public double Bearing
            {
                get { return mBearing; }
                set { mBearing = value; }
            }



            private string mVehicleState;

            public string VehicleState
            {
                get { return mVehicleState; }
                set { mVehicleState = value; }
            }


            private string mMessageFromDriver;

            public string MessageFromDriver
            {
                get { return mMessageFromDriver; }
                set { mMessageFromDriver = value; }
            }

            private bool mFreeKm;

            public bool FreeKm
            {
                get { return mFreeKm; }
                set { mFreeKm = value; }
            }

    }


    public class JsonOrderResponse
    {

        private long mIdVehicle;

        public long IdVehicle
        {
            get { return mIdVehicle; }
            set { mIdVehicle = value; }
        }


        private string mPlate;

        public string Plate
        {
            get { return mPlate; }
            set { mPlate = value; }
        }


        private string mDescriptionShort;

        public string DescriptionShort
        {
            get { return mDescriptionShort; }
            set { mDescriptionShort = value; }
        }

        private double mLongitudeX;

        public double LongitudeX
        {
            get { return mLongitudeX; }
            set { mLongitudeX = value; }
        }


        private double mLatitudeY;

        public double LatitudeY
        {
            get { return mLatitudeY; }
            set { mLatitudeY = value; }
        }


        private double mBearing;

        public double Bearing
        {
            get { return mBearing; }
            set { mBearing = value; }
        }

        private long mIdOrder;

        public long IdOrder
        {
            get { return mIdOrder; }
            set { mIdOrder = value; }
        }

        private int mTimeToPickUp;

        public int TimeToPickUp
        {
            get { return mTimeToPickUp; }
            set { mTimeToPickUp = value; }
        }
    }


    // Ovaa class e za Overview na vozilata
    public class JsonVehiclesStates
    {

        private long mIdVehicle;

        public long IdVehicle
        {
            get { return mIdVehicle; }
            set { mIdVehicle = value; }
        }


        private string mPlate;

        public string Plate
        {
            get { return mPlate; }
            set { mPlate = value; }
        }


        private string mDescriptionShort;

        public string DescriptionShort
        {
            get { return mDescriptionShort; }
            set { mDescriptionShort = value; }
        }

        private double mLongitudeX;

        public double LongitudeX
        {
            get { return mLongitudeX; }
            set { mLongitudeX = value; }
        }


        private double mLatitudeY;

        public double LatitudeY
        {
            get { return mLatitudeY; }
            set { mLatitudeY = value; }
        }


        private double mBearing;

        public double Bearing
        {
            get { return mBearing; }
            set { mBearing = value; }
        }


        private string mSpeed;

        public string Speed
        {
            get { return mSpeed; }
            set { mSpeed = value; }
        }


        private string mVehicleState;

        public string VehicleState
        {
            get { return mVehicleState; }
            set { mVehicleState = value; }
        }


        private int mVehicleStateColor;

        public int VehicleStateColor
        {
            get { return mVehicleStateColor; }
            set { mVehicleStateColor = value; }
        }


        private string mDriver;

        public string Driver
        {
            get { return mDriver; }
            set { mDriver = value; }
        }

        private string mDriverStart;

        public string DriverStart
        {
            get { return mDriverStart; }
            set { mDriverStart = value; }
        }

        private string mTaximeter;

        public string Taximeter
        {
            get { return mTaximeter; }
            set { mTaximeter = value; }
        }

        private string mLastLocation;

        public string LastLocation
        {
            get { return mLastLocation; }
            set { mLastLocation = value; }
        }


        private string mStanica;

        public string Stanica
        {
            get { return mStanica; }
            set { mStanica = value; }
        }

        private string mRegion;

        public string Region
        {
            get { return mRegion; }
            set { mRegion = value; }
        }

        private string mDoRegion;

        public string DoRegion
        {
            get { return mDoRegion; }
            set { mDoRegion = value; }
        }


        private string mAdresa1;

        public string Adresa1
        {
            get { return mAdresa1; }
            set { mAdresa1 = value; }
        }
    }


    public class JsonReservation
    {

        private long mIdReservation;

        public long IdReservation
        {
            get { return mIdReservation; }
            set { mIdReservation = value; }

        }
        private string mStreetName;

        public string StreetName
        {
            get { return mStreetName; }
            set { mStreetName = value; }
        }


        private int mStreetNumber;

        public int StreetNumber
        {
            get { return mStreetNumber; }
            set { mStreetNumber = value; }
        }     


        private string mPickupAdress;

        public string PickupAdress
        {
            get { return mPickupAdress; }
            set { mPickupAdress = value; }
        }


        private string mTo;

        public string To
        {
            get { return mTo; }
            set { mTo = value; }
        }


        private string mComment;

        public string Comment
        {
            get { return mComment; }
            set { mComment = value; }
        }


        private string mCompanies;

        public string Companies
        {
            get { return mCompanies; }
            set { mCompanies = value; }
        }


        private double mLongitudeX;

        public double LongitudeX
        {
            get { return mLongitudeX; }
            set { mLongitudeX = value; }
        }


        private double mLatitudeY;

        public double LatitudeY
        {
            get { return mLatitudeY; }
            set { mLatitudeY = value; }
        }


        private string mImei;

        public string Imei
        {
            get { return mImei; }
            set { mImei = value; }
        }


        private string mReservationPickUpTime;

        public string ReservationPickUpTime
        {
            get { return mReservationPickUpTime; }
            set { mReservationPickUpTime = value; }
        }
    }
}
