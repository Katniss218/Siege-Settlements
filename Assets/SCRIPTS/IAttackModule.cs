namespace SS.Objects.Modules
{
	public interface IAttackModule
	{
		/// <summary>
		/// Returns the max view distance of the target finder.
		/// </summary>
		float attackRange { get; }
		
		bool isReadyToAttack { get; }

		//Targeter targeter { get; }
		SSObjectDFSC target { get; set; }
		
		void FindTargetClosest();

		void TrySetTarget( SSObjectDFSC target );
	}
}