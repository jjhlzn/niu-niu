using System;


public class GameState
{
	public static GameState BeforeStart = new GameState(1);   //房主点按开始之前
	public static GameState Ready = new GameState(2); //房主点按开始之后，并且所有玩家已经准本好
	public static GameState FirstDeal = new GameState(3); //第一次分牌中
	public static GameState RobBanker = new GameState(4); //抢专
	public static GameState ChooseBanker = new GameState(10); //抢专
	public static GameState SecondDeal = new GameState(5); //第二次发牌
	public static GameState CheckPoker = new GameState(6); //查看手牌
	public static GameState ComparePoker = new GameState(7); //查看手牌
	public static GameState GameBeforeStart = new GameState(8); //一局结束
	public static GameState GameOver = new GameState(9);

	private int value;

	private GameState (int value)
	{
		this.value = value;
	}
		
	public GameState nextState() {
		GameState next = null;
		if (this.Equals(BeforeStart)) {
			next = Ready;
		} else if (this.Equals (Ready)) {
			next = FirstDeal;
		} else if (this.Equals (FirstDeal)) {
			next = RobBanker;
		} else if (this.Equals (RobBanker)) {
			next = ChooseBanker;
		} else if (this.Equals (ChooseBanker)) {
			next = SecondDeal;
		} else if (this.Equals (SecondDeal)) {
			next = CheckPoker;
		} else if (this.Equals (CheckPoker)) {
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
		return this.value;
	} 
}


