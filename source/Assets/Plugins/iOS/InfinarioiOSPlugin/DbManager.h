//
//  DbManager.h
//  ToDoList
//
//  Created by Igi on 2/3/15.
//  Copyright (c) 2015 Igi. All rights reserved.
//

#import <Foundation/Foundation.h>

extern NSString *const DB_FILE;

@interface DbManager : NSObject

@property (nonatomic, strong) NSMutableArray *arrColumnNames;
@property (nonatomic) int affectedRows;
@property (nonatomic) long long lastInsertedRowId;

- (instancetype)initWithDatabaseFilename:(NSString *)dbFilename;
- (NSArray *)loadDataFromDb:(NSString *)query;
- (void)executeQuery:(NSString *)query;

@end
