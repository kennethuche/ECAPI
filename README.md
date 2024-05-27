# My .NET Core API Application

## Introduction

This is a simple .NET Core API application that provides a backend for managing student records. The application supports operations such as creating, updating, and deleting students, as well as booking holidays for students.

## Getting Started

### Clone the Repository

First, clone the repository to your local machine using Git:

```sh
git clone https://github.com/kennethuche/ECAPI.git
cd ECAPI
dotnet run



## question 2

'''
function BookHolidayAsync(studentId, holidayStart, holidayEnd)
    if holidayStart is not Monday or holidayEnd is not Friday then
        throw "Holiday must start on a Monday and end on a Friday"
    end if

    student = await GetStudentByIdAsync(studentId)
    if student is null then
        throw "Student does not exist"
    end if

    daysToExtend = (holidayEnd - holidayStart) + 1

    adjustedCourses = empty list
    coursesToAdjust = sort student.Courses by StartDate
    endOfHolidayAdjustedCourse = holidayEnd
    minAdjustedStartDate = MaxDate
    maxAdjustedEndDate = MinDate

    for each course in coursesToAdjust do
        if course overlaps with holiday then
            adjust course start date to 3 days after endOfHolidayAdjustedCourse
            adjust course end date by adding daysToExtend
            endOfHolidayAdjustedCourse = course.EndDate

            update minAdjustedStartDate if course.StartDate is earlier
            update maxAdjustedEndDate if course.EndDate is later

            if course does not start on Monday or end on Friday then
                throw "Adjusted courses must start on a Monday and end on a Friday"
            end if

            add course to adjustedCourses
        else if course does not overlap with holiday then
            add course to adjustedCourses
        end if
    end for

    if adjustedCourses is not empty then
        remove adjustedCourses from coursesToAdjust
    end if

    adjust remaining courses in coursesToAdjust to avoid overlap

    create holiday record with holidayStart, holidayEnd, studentId
    add holiday record to database

    await save changes to database
end function
''''

