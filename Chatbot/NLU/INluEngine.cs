namespace Chatbot.NLU;

/**
 * Interface for NLU-motorer
 * Så vi kan skifte mellem forskellige NLU-implementeringer
 * uden at ændre resten af systemet (god SoC og DI)
 */
public interface INluEngine {
    Task<NluResult> PredictAsync(string input);
}