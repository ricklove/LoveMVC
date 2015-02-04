

# Hours

## Untracked

- Create prototype

## Create LoveTemplate

### 2015-02-03 11:05-11:15

- Create task list

### 11:16-12:00

- Create project structure without dependencies
- Parse Razor

### 12:31-14:45

- Create LoveSyntaxTree from Razor syntax tree

### 15:30-18:00

- Create Mvc MarkupExpressionEvaluator

### 2015-02-04 6:30-8:20

- Create Mvc MarkupExpressionEvaluator
- Host in Asp.Net MVC

### 8:21-8:45

- Execute the code in a view...

### 9:45-13:00

- Evaluation almost working 

# Tasks

## Generate Templates

- Remove Dependencies from code
- Setup Minimal Love Template Generation Environment

## Convert Templates to Pure Html

## Convert Templates to Knockout Client

## Convert Templates to Asp.Net Mvc with Client Selection



// System.Web.Mvc.Html.TemplateHelpers
internal static Dictionary<string, Func<HtmlHelper, string>> GetDefaultActions(DataBoundControlMode mode)
{
	if (mode != DataBoundControlMode.ReadOnly)
	{
		return TemplateHelpers._defaultEditorActions;
	}
	return TemplateHelpers._defaultDisplayActions;
}
