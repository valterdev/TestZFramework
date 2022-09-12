namespace ZFramework
{
    public abstract class UIPanel : UIElement
    {
        #region Methods

        // ---------------------------------------------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------------------------------------------

        // Show (priority > 0) / Hide (priority <= 0)
        // performed in the form of priorities due to the fact that different elements can impose different
        // requirements to show/hide the panel, and conflicts are resolved through priorities
        public void Require(object token, int priority, bool immediately = false)
        {
            UIBase.Require(this, token, priority, immediately);
        }


        public void Open()
        {
            Require(this, +1);

            // TODO Not working correctly with 'bool immediately = true', hides panels in next state
            // Require(this, +1, true);
        }


        public void Close()
        {
            Require(this, 0);

            // Require(this, 0, true);
        }

        #endregion
    }
}