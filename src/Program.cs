using System;
using System.Collections.Generic;
using System.Linq;

public class Validator
{
    public int ID { get; private set; }
    public int DepositAmount { get; private set; }
    public List<int> Votes { get; private set; }

    public Validator(int id, int depositAmount)
    {
        ID = id;
        DepositAmount = depositAmount;
        Votes = new List<int>();
    }

    public void CastVote(int checkpoint)
    {
        // Simulate random choice between left and right child
        Random rand = new Random();
        int choice = rand.Next(0, 2); // 0 for left, 1 for right
        var leftChild = 2 * checkpoint + 1;
        var rightChild = 2 * checkpoint + 2;
        Votes.Add(choice == 0 ? leftChild : rightChild);
    }
}

public class PoSVotingSystem
{
    private List<Validator> validators;
    private Dictionary<int, List<int>> checkpointVotes;

    public PoSVotingSystem()
    {
        validators = new List<Validator>();
        checkpointVotes = new Dictionary<int, List<int>>();
    }

    public void AddValidator(int id, int depositAmount)
    {
        validators.Add(new Validator(id, depositAmount));
    }

    public void SimulateVoting(int totalCheckpoints)
    {
        for (int i = 0; i < totalCheckpoints; i++)
        {
            foreach (Validator validator in validators)
            {
                validator.CastVote(i); // Each validator casts a vote for the current checkpoint 'i'
            }

            // Calculate supermajority link
            var votes = validators.SelectMany(v => v.Votes).ToList(); // Get all votes from all validators for this checkpoint
            var voteSum = validators.Sum(v => v.DepositAmount); // Calculate the total deposit amount of all validators
            var supermajorityLink = votes.GroupBy(v => v)
                .Where(g => g.Sum(v => validators.FirstOrDefault(validator => validator.Votes.Contains(v))?.DepositAmount ?? 0) > voteSum / 2) // Find the vote that surpasses the majority threshold
                .OrderByDescending(g => g.Sum(v => validators.FirstOrDefault(validator => validator.Votes.Contains(v))?.DepositAmount ?? 0)) // Order by the sum of deposit amounts for each vote
                .FirstOrDefault()?
                .Key; // Get the first (highest) supermajority link if found

            if (supermajorityLink != null)
            {
                checkpointVotes[i] = new List<int> { supermajorityLink.Value }; // Store the supermajority link for this checkpoint
            }
        }
    }


    public void PrintBlockchainAndFinalizedCheckpoints()
    {
        Console.WriteLine("Blockchain formed by supermajority links:");
        foreach (var kvp in checkpointVotes)
        {
            Console.WriteLine($"Checkpoint {kvp.Key}: {string.Join(", ", kvp.Value)}");
        }

        Console.WriteLine("\nFinalized checkpoints by validators:");
        foreach (var kvp in checkpointVotes)
        {
            Console.WriteLine($"Checkpoint {kvp.Key} finalized by validators: {string.Join(", ", validators.Where(v => v.Votes.Contains(kvp.Value[0])).Select(v => v.ID))}");
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        PoSVotingSystem votingSystem = new PoSVotingSystem();

        // fixed validators with their deposit amounts
        int[] depositAmounts = { 500, 100, 300, 250, 150, 500, 600, 350, 200, 150 };
        for (int x = 0; x < 10; x++)
        {
            votingSystem.AddValidator(x, depositAmounts[x]);
        }

        int totalCheckpoints = 10; // Total number of checkpoints
        votingSystem.SimulateVoting(totalCheckpoints);
        votingSystem.PrintBlockchainAndFinalizedCheckpoints();
    }
}
