using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AgentParameters", order = 1)]
public class GridWorldTrainingParams : ScriptableObject
{
	// Number of training episodes to run before reducing the frame rate.
	public int numTrainEpisodes;

	// The discount.
	[Range(0.0f, 1.0f)]
	public float gamma;

	// The exploration rate starting value.
	[Range(0.0f, 1.0f)]
	public float epsilonStart;

	// The exploration rate final value.
	[Range(0.0f, 1.0f)]
	public float epsilonEnd;

	// The learning rate.
	[Range(0.0f, 1.0f)]
	public float alpha;

	public float intrinsicRewardStrength;
}
