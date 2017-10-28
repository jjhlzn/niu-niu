using System;


public class GameState
{
	public static GameState BeforeStart = new GameState("BeforeStart");   //房主点按开始之前
	public static GameState Ready = new GameState("Ready"); //房主点按开始之后，并且所有玩家已经准本好
	public static GameState FirstDeal = new GameState("FirstDeal"); //第一次分牌中
	public static GameState RobBanker = new GameState("RobBanker"); //抢专
	public static GameState ChooseBanker = new GameState("ChooseBanker"); //抢专
	public static GameState Bet = new GameState("Bet");
	public static GameState SecondDeal = new GameState("SecondDeal"); //第二次发牌
	public static GameState CheckCard = new GameState("CheckCard"); //查看手牌
	public static GameState ComparePoker = new GameState("ComparePoker"); //查看手牌
	public static GameState GameBeforeStart = new GameState("GameBeforeStart"); //一局结束
	public static GameState GameOver = new GameState("GameOver");

	public string value;

	private GameState (string value)
	{
		this.value = value;
	}
		
	public GameState nextState() {
		GameState next = null;
		if (this.Equals (BeforeStart)) {
			next = Ready;
		} else if (this.Equals (Ready)) {
			next = FirstDeal;
		} else if (this.Equals (FirstDeal)) {
			next = RobBanker;
		} else if (this.Equals (RobBanker)) {
			next = ChooseBanker;
		} else if (this.Equals (ChooseBanker)) {
			next = Bet;
		} else if (this.Equals (Bet)) {
			next = SecondDeal;
		} else if (this.Equals (SecondDeal)) {
			next = CheckCard;
		} else if (this.Equals (CheckCard)) {
			next = ComparePoker;
		} else if (this.Equals (ComparePoker)) {
			next = GameBeforeStart;
		} else if (this.Equals (GameBeforeStart)) {
			next = Ready;
		} 
		return next;
	}

	public override bool Equals(object obj)
	{
		GameState p = (GameState)obj;
		return this.value == p.value;
	} 

	public override int GetHashCode()
	{
		return this.value.GetHashCode();
	} 
}


