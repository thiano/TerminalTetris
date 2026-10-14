namespace TerminalTetris.Input;

/*
 * Interface voor het niet-blokkerend afhandelen van spelerinvoer
 */
public interface iInputHandler
{
    /*
     * Pollt de invoerbuffer en retourneert de eerstvolgende geregistreerde gameAction
     * Als er geen toets ingedrukt is, levert dit gemeAction.None op zonder te blokkeren
     */
    gameAction PollAction();

}