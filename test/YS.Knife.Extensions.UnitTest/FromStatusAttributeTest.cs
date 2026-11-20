namespace YS.Knife.Extensions.UnitTest
{
    public class FromStatusAttributeTest
    {
        [Fact]
        public void ShouldAllowTransitionWhenCurrentInTargetValues()
        {
            FromStatusAttribute.IsValidTransition(AuditState.New, AuditState.Doing).Should().BeTrue();
            FromStatusAttribute.IsValidTransition(AuditState.Doing, AuditState.Completed).Should().BeTrue();
            FromStatusAttribute.IsValidTransition(AuditState.Failed, AuditState.Completed).Should().BeTrue();
            FromStatusAttribute.IsValidTransition(AuditState.Rejected, AuditState.Completed).Should().BeTrue();
            FromStatusAttribute.IsValidTransition(AuditState.New, AuditState.Rejected).Should().BeTrue();
            FromStatusAttribute.IsValidTransition(AuditState.Doing, AuditState.Failed).Should().BeTrue();
        }
        [Fact]
        public void ShouldDenyTransitionWhenCurrentNotInTargetValues()
        {
            FromStatusAttribute.IsValidTransition(AuditState.Completed, AuditState.Doing).Should().BeFalse();
            FromStatusAttribute.IsValidTransition(AuditState.New, AuditState.Completed).Should().BeFalse();
            FromStatusAttribute.IsValidTransition(AuditState.Rejected, AuditState.Doing).Should().BeFalse();
            FromStatusAttribute.IsValidTransition(AuditState.New, AuditState.Failed).Should().BeFalse();
        }
        [Fact]
        public void ShouldAllowAnyTransitionWhenTargetFieldHasNoAttribute()
        {
            FromStatusAttribute.IsValidTransition(AuditState.Completed, AuditState.New).Should().BeTrue();
            FromStatusAttribute.IsValidTransition(AuditState.Doing, AuditState.New).Should().BeTrue();
        }
        [Fact]
        public void ShouldAllowTransitionWhenTargetValueIsNotDefinedInEnum()
        {
            FromStatusAttribute.IsValidTransition(AuditState.New, (AuditState)99).Should().BeTrue();
        }
        [Fact]
        public void ShouldAllowSelfTransitionWhenCurrentInValues()
        {
            FromStatusAttribute.IsValidTransition(AuditState.Completed, AuditState.Completed).Should().BeTrue();
        }
        [Fact]
        public void ShouldThrowWhenAttributeValueIsNotSameEnumType()
        {
            Action act = () => FromStatusAttribute.IsValidTransition(AuditState.New, AuditState.WrongTyped);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*AuditState.WrongTyped*");
        }
        public enum AuditState
        {
            New = 0,
            [FromStatus(New)]
            Doing = 1,
            [FromStatus(Doing, Failed, Rejected, Completed)]
            Completed = 2,
            [FromStatus(New, Doing)]
            Rejected = 3,
            [FromStatus(Doing)]
            Failed = 6,
            [FromStatus(1)]
            WrongTyped,
        }
    }
}
