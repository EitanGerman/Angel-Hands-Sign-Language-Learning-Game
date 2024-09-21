using Assets.Scripts.User;
using Assets.Scripts.CommonTypes;
using Unity.VisualScripting;


namespace Assets.Scripts.Statistics { 
public class UserStatistics
{
    private int totalGamesPlayed = 0;
    private int heightstScore = 0;
    private int wordsLearned = 0;

    public UserStatistics(int totalGamesPlayed, int totalWins, int wordsLearne)
    {
        this.totalGamesPlayed = totalGamesPlayed;
        this.heightstScore = totalWins;
        this.wordsLearned = wordsLearne;
    }
    public UserStatistics()
    {
    }

    public int getWordLeaned()
    {
        return wordsLearned;
    }

    public void learnNewWord()
    {
       wordsLearned++;
    }

    public void forgetWord()
    {
        wordsLearned--;
    }

    public void gamePlayed()
    {
        totalGamesPlayed++;
    }

}
}