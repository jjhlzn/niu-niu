using System;

[System.Serializable]
public class GameState
{
	/****
	 *  游戏状态流转：
	 *  1. BeforeStart
	 *  2. FirstDeal
	 *  3. RobBanker
	 *  4. ChooseBanker (只有客户端有这个状态，服务器上是没有的)
	 *  5. Bet
	 *  6. SecondDeal
	 *  7. CheckCard  （如果有下一轮，那么会跳到CompareCard，如果没有下一轮就跳到GameOver）
	 *  8. CompareCard  (只有客户端有这个状态，服务器上没有，比牌的动画结束后，跳到WaitForNextRound状态)
	 *  9. WaitForNextRound
	 *  9. GameOver  
	 * */
	public static GameState BeforeStart = new GameState("BeforeStart");   //房主点按开始之前
	//public static GameState Ready = new GameState("Ready"); //房主点按开始之后，并且所有玩家已经准本好
	public static GameState FirstDeal = new GameState("FirstDeal"); //第一次分牌中
	public static GameState RobBanker = new GameState("RobBanker"); //抢专
	public static GameState ChooseBanker = new GameState("ChooseBanker"); //选择庄家
	public static GameState Bet = new GameState("Bet");
	public static GameState SecondDeal = new GameState("SecondDeal"); //第二次发牌
	public static GameState CheckCard = new GameState("CheckCard"); //查看手牌
	public static GameState CompareCard = new GameState("CompareCard"); //比牌
	public static GameState WaitForNextRound = new GameState("WaitForNextRound"); 
	public static GameState GameOver = new GameState("GameOver");

	public string value;

	private GameState (string value)
	{
		this.value = value;
	}
		
	/*
	public GameState nextState() {
		GameState next = null;
		if (this.Equals (BeforeStart)) {
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
			next = CompareCard;
		} else if (this.Equals (CompareCard)) {
			next = FirstDeal;
		} 
		return next;
	} */

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


