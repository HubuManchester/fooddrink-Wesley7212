namespace FlavorQuest.Services;

/// <summary>
/// Wraps the MAUI TextToSpeech API to provide spoken recipe reading for accessibility.
/// Supports reading ingredients, instructions, and general text aloud.
/// </summary>
public class TextToSpeechService
{
    private bool _isSpeaking;
    private CancellationTokenSource? _cts;

    /// <summary>
    /// Indicates whether the service is currently speaking.
    /// </summary>
    public bool IsSpeaking => _isSpeaking;

    /// <summary>
    /// Speaks the given text aloud using the device's text-to-speech engine.
    /// Includes error handling for unsupported platforms.
    /// </summary>
    /// <param name="text">The text to speak.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    public async Task SpeakAsync(string text, CancellationToken? cancellationToken = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            _cts = cancellationToken != null
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value)
                : new CancellationTokenSource();

            _isSpeaking = true;

            var options = new SpeechOptions
            {
                Pitch = 1.0f,
                Volume = 1.0f
            };

            await TextToSpeech.SpeakAsync(text, options, _cts.Token);
        }
        catch (TaskCanceledException)
        {
            // Speech was cancelled — normal behavior, not an error
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Text-to-Speech error: {ex.Message}");
            // Fail silently — TTS is an accessibility enhancement, not critical functionality
        }
        finally
        {
            _isSpeaking = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    /// <summary>
    /// Speaks a recipe's ingredients list aloud, formatted for clarity.
    /// </summary>
    /// <param name="ingredients">List of ingredient strings.</param>
    public async Task SpeakIngredientsAsync(List<string> ingredients)
    {
        if (ingredients == null || ingredients.Count == 0)
            return;

        var text = "Ingredients needed: " + string.Join(". ", ingredients);
        await SpeakAsync(text);
    }

    /// <summary>
    /// Speaks a recipe's instructions step by step.
    /// </summary>
    /// <param name="instructions">List of instruction strings.</param>
    public async Task SpeakInstructionsAsync(List<string> instructions)
    {
        if (instructions == null || instructions.Count == 0)
            return;

        var sb = new System.Text.StringBuilder("Instructions. ");
        for (int i = 0; i < instructions.Count; i++)
        {
            sb.Append($"Step {i + 1}: {instructions[i]}. ");
        }
        await SpeakAsync(sb.ToString());
    }

    /// <summary>
    /// Stops any ongoing speech immediately.
    /// </summary>
    public void Stop()
    {
        try
        {
            _cts?.Cancel();
            _isSpeaking = false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping TTS: {ex.Message}");
        }
    }
}
