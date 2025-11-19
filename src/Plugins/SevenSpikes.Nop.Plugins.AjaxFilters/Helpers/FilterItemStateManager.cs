using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public class FilterItemStateManager
{
	public static FilterItemState GetNewStateBaseOnOptionAvailability(FilterItemState currentState, bool optionAvailable)
	{
		return GetNewStateBaseOnOptionAvailabilityInternal(currentState, optionAvailable);
	}

	private static FilterItemState GetNewStateBaseOnOptionAvailabilityInternal(FilterItemState currentState, bool optionAvailable)
	{
		FilterItemState result = currentState;
		switch (currentState)
		{
		case FilterItemState.Unchecked:
			if (!optionAvailable)
			{
				result = FilterItemState.Disabled;
			}
			break;
		case FilterItemState.Checked:
			if (!optionAvailable)
			{
				result = FilterItemState.CheckedDisabled;
			}
			break;
		case FilterItemState.CheckedDisabled:
			if (optionAvailable)
			{
				result = FilterItemState.Checked;
			}
			break;
		case FilterItemState.Disabled:
			if (optionAvailable)
			{
				result = FilterItemState.Unchecked;
			}
			break;
		}
		return result;
	}
}
