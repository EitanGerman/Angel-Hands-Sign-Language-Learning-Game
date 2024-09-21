using static Assets.Scripts.Utills.Communication.Communication;
using Assets.Scripts.Utills.Communication.EnvVar;
using Assets.Scripts.Utills.Communication.Files;
using Assets.Scripts.Utills.Communication.Port;
using UnityEngine;


namespace Assets.Scripts.Utills.Communication
{

    ////////////////////////////////////////////////////////////////////////
    /*
    ICommunication -  define methods for communication
    define basline for communication
    */
    public interface ICommunication
    {
        /*
         StartCommunication : start listening to the source on a thread
        */
        public void StartCommunication();

        /*
        Destroy : abort the thread and stop listening
        */
        public void Destroy();

    }//interface communication


    ////////////////////////////////////////////////////////////////////////
    /*
     CommunicationType -  define types of communication available
     */
    public enum CommunicationType
    {
        ENV_VAR,
        FILE,
        PORT,
        NONE
    }
    /*
     Communication -  define properties and methods for communication based on ICommunication
     include source and data for data transfer
     source - where to search the data
     data - output data to be transfered through communication
     */
    public abstract class Communication : ICommunication
    {
        public abstract string source {get; set;}
        public abstract string data { get; set; }

        public abstract void StartCommunication();

        public abstract void Destroy();

    }//class communication


    ////////////////////////////////////////////////////////////////////////
    /*
     when no input source is set
     */
    public class empty : Communication
    {
        public override string source {get;set;}
        public override string data { get; set; }
        public override void Destroy()
        {
            return;
        }
        public override void StartCommunication()
        {
            return;
        }
    }


    ////////////////////////////////////////////////////////////////////////
    /*
     CommunicationFactory -  create communication based on type
     used for genric purpose
     */
    public static class CommunicationFactory
    {
        public static Communication CreateCommunication(CommunicationType type)
        {
            switch (type)
            {
                case CommunicationType.ENV_VAR:
                    return new EnvironmentVariableWatcher();
                case CommunicationType.FILE:
                    return new FileReader();
                case CommunicationType.PORT:
                    return new PortListener();
                case CommunicationType.NONE:
                     return new empty();
                default:
                    throw new System.ArgumentException("Invalid communication type");
            }
        }
    }//CommunicationFactory


    ////////////////////////////////////////////////////////////////////////
    /*
     CommunicationManager -  manage communication
     initiate, destroy and get output
     */
    public class CommunicationManager
    {
        Communication asl_input;

        /*
         create the connection based on type and source
         */
        public void initiate(CommunicationType communicationType,string input_source)
        {
            asl_input = CommunicationFactory.CreateCommunication(communicationType);
            asl_input.source = input_source;
            asl_input.StartCommunication();
        }
        public void destroy()
        {
            asl_input.Destroy();
        }
        public string getOutput()
        {
            return asl_input.data;
        }
    }
}