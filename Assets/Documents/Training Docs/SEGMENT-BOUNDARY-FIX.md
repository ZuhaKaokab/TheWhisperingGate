# Fixing Segment Boundary Issue

## Problem
When using Option B (multiple trees), after completing Segment 1 (jungle_scream node), the dialogue continues into Segment 2 instead of ending.

## Root Cause
The `jungle_scream` node in `Tree_JungleAwakening` has choices that still point to nodes that are now in `Tree_WriterMeeting` (jungle_follow_scream, jungle_wait, jungle_trap → jungle_writer_meeting).

## Solution

You have **two options**:

### Option 1: Mark Node as End Node (RECOMMENDED)
1. Open `Tree_JungleAwakening` in the Dialogue Editor
2. Select the `jungle_scream` node
3. Check **"Is End Node"** checkbox
4. Set **"Display Duration"** to 0 (or leave default)
5. This will make the dialogue end after the player makes a choice

### Option 2: Remove Next Node Links
1. Open `Tree_JungleAwakening` in the Dialogue Editor
2. Select the `jungle_scream` node
3. For each choice:
   - Click on the choice
   - Set **"Next Node"** field to **None** (empty)
4. This will end the dialogue when any choice is selected

## Why This Happens

When you split the dialogue into multiple trees:
- `Tree_JungleAwakening` should contain nodes 1-8 only
- `Tree_WriterMeeting` should contain nodes 9-15 only

But if the nodes in Tree_JungleAwakening still have their "Next Node" fields pointing to nodes that are now in Tree_WriterMeeting, the dialogue system will try to continue, which can cause issues.

## Recommended Setup for Segment 1

**Tree_JungleAwakening:**
- Start Node: `jungle_start`
- Last Node: `jungle_scream`
- `jungle_scream` should be marked as **End Node** OR have all choices' Next Node set to None

**Tree_WriterMeeting:**
- Start Node: `jungle_writer_meeting` (or create a new intro node)
- This tree starts fresh when Segment 2 is triggered

## Testing

After making the change:
1. Trigger Segment 1 dialogue
2. Go through all nodes
3. When you reach `jungle_scream` and make a choice, the dialogue should **END**
4. Segment 1 should be marked as complete
5. You should be able to move to Segment 2's trigger and start that dialogue separately

## Code Fix Applied

I've also added code to automatically end dialogue if a choice leads to a null node, which provides a safety net. But the proper fix is to configure your trees correctly as described above.


