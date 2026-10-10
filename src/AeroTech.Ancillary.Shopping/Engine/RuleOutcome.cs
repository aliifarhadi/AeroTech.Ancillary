using AeroTech.Ancillary.Shopping.Results;

namespace AeroTech.Ancillary.Shopping.Engine
{
    internal enum RuleVerdict
    {
        Match = 1,
        NoMatch = 2,
        Insufficient = 3
    }

    internal sealed class RuleOutcome
    {
        private readonly List<string> _reasons = [];
        private readonly List<string> _notes = [];
        private readonly List<string> _missing = [];
        private readonly List<SelectionFieldIssue> _issues = [];
        private bool _unknown;
        private bool _noMatch;
        private bool _pending;
        private bool _pendingRule;

        public RuleVerdict Verdict => _unknown ? RuleVerdict.Insufficient : _noMatch ? RuleVerdict.NoMatch : _pending ? RuleVerdict.Insufficient : RuleVerdict.Match;

        public bool OnlySelectionPending => !_unknown && !_noMatch && _pending;

        public bool RulesDecided => !_unknown && !_pendingRule;

        public IReadOnlyList<string> ReasonCodes => _reasons.Concat(_notes).Distinct(StringComparer.Ordinal).ToList();

        public IReadOnlyList<string> MissingFields => _missing.Distinct(StringComparer.Ordinal).ToList();

        public IReadOnlyList<SelectionFieldIssue> FieldIssues => _issues.Distinct().ToList();

        public void Fail(string reasonCode)
        {
            _noMatch = true;
            _reasons.Add(reasonCode);
        }

        public void Unknown(string reasonCode, string field)
        {
            _unknown = true;
            _reasons.Add(reasonCode);
            _missing.Add(field);
        }

        public void AwaitSelection(string reasonCode, string field)
        {
            _pendingRule = true;
            AwaitForm(reasonCode, field);
        }

        public void AwaitForm(string reasonCode, string field)
        {
            _pending = true;
            _reasons.Add(reasonCode);
            _missing.Add(field);
            _issues.Add(new SelectionFieldIssue(field, reasonCode));
        }

        public void Reject(string field, string reasonCode)
        {
            _noMatch = true;
            _reasons.Add(reasonCode);
            _issues.Add(new SelectionFieldIssue(field, reasonCode));
        }

        public void Note(string reasonCode) => _notes.Add(reasonCode);
    }
}
