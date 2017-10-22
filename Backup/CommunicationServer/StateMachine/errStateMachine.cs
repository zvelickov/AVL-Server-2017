using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;


namespace Taxi.Communication.Server.StateMachine
{
    class errStateMachine
    {
        public static void LogStateMachineError(long IdVehicle, string CurrentState, string MethodNotImplemented, string Comment)
        {
            string ErrorMessage = "Method Not Implemented, " + IdVehicle.ToString() + ", " + CurrentState + ", " + MethodNotImplemented + ", " + Comment;
            Console.WriteLine(ErrorMessage);
            ServiceCallBack.log.Error(ErrorMessage);
        }

        public static void LogStateMachineError(long IdVehicle, string CurrentState, string CommentText)
        {
            string ErrorMessage = "ERROR" + IdVehicle.ToString() + ", " + CurrentState + ", " + CommentText;
            Console.WriteLine(ErrorMessage);
            ServiceCallBack.log.Error(ErrorMessage);
        }
    }
}
