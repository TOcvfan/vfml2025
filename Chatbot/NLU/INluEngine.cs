namespace Chatbot.NLU;

using Chatbot.Models;

/** 
* Interface for NLU-motorer
* Så vi kan skifte mellem forskellige NLU-implementeringer 
* uden at ændre resten af systemet (god SoC og DI)
*/
public interface INluEngine {
    NluResult Predict(string input);
}