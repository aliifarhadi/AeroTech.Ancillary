namespace AeroTech.Ancillary.Stage2.EndToEndTests.Scenario;

public sealed record TranscriptEntry(int Sequence, string Caller, string Method, string Route, string? Request, int Status, string? Response, double ElapsedMs);
