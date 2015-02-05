

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

### 14:30-15:00

- Debug Evaluation

### 2015-02-05 4:00-5:00

- Add Evaluation Scope

### 5:01-6:35

- Got HtmlHelpers working without IIS host

### 6:36-6:55

- False alarm, still requires IIS

### 6:56-7:15

- Clean up project

# Tasks


## Generate Templates

- Remove Dependencies from code
- Setup Minimal Love Template Generation Environment
- Create Test environment (Still requires IIS because of Virtual Path Providers)

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
